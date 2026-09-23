using System.Net;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Chat.Core;
using Chat.Core.Modelle;

namespace Chat.Adapter.ModelRunner;

public sealed class ModelRunnerClient : IModelServerClient
{
    private readonly HttpClient _http;
    private readonly Konfiguration _konfiguration;

    public ModelRunnerClient(HttpClient http, Konfiguration konfiguration)
    {
        _http = http;
        _konfiguration = konfiguration;
    }

    public async IAsyncEnumerable<string> StreamAntwortAsync(
        IReadOnlyList<Nachricht> verlauf,
        string systemanweisung,
        [EnumeratorCancellation] CancellationToken ct)
    {
        _konfiguration.Pruefen();
        using HttpRequestMessage anfrage = BaueAnfrage(verlauf, systemanweisung);
        using HttpResponseMessage antwort = await SendeAsync(anfrage, ct);

        if (!antwort.IsSuccessStatusCode)
        {
            throw await ErzeugeStoerfallAsync(antwort, ct);
        }

        await using Stream stream = await OeffneStreamAsync(antwort.Content, ct);
        using StreamReader reader = new(stream);
        bool regulaerBeendet = false;

        while (await LeseZeileAsync(reader, ct) is { } zeile)
        {
            SseEreignis ereignis = LeseEreignis(zeile);
            if (ereignis.IstEnde)
            {
                regulaerBeendet = true;
                break;
            }

            if (!string.IsNullOrEmpty(ereignis.Teil))
            {
                yield return ereignis.Teil;
            }
        }

        if (!regulaerBeendet)
        {
            throw new StoerfallException(
                Stoerfall.ModellserverNichtErreichbar,
                "Der Antwortstream wurde vor dem regulären Abschluss beendet.");
        }
    }

    private HttpRequestMessage BaueAnfrage(
        IReadOnlyList<Nachricht> verlauf,
        string systemanweisung)
    {
        ChatAnfrage inhalt = new(
            _konfiguration.Modellname,
            BaueVerlauf(verlauf, systemanweisung),
            true,
            _konfiguration.Temperatur,
            _konfiguration.MaximaleAntwortlaenge);

        return new HttpRequestMessage(HttpMethod.Post, BaueZieladresse())
        {
            Content = JsonContent.Create(inhalt)
        };
    }

    private Uri BaueZieladresse()
    {
        string basis = _konfiguration.AdresseModellserver.TrimEnd('/') + "/";
        return new Uri(new Uri(basis, UriKind.Absolute), "chat/completions");
    }

    private async Task<HttpResponseMessage> SendeAsync(
        HttpRequestMessage anfrage,
        CancellationToken ct)
    {
        try
        {
            return await _http.SendAsync(
                anfrage,
                HttpCompletionOption.ResponseHeadersRead,
                ct);
        }
        catch (HttpRequestException)
        {
            throw new StoerfallException(
                Stoerfall.ModellserverNichtErreichbar,
                "Der lokale Modellserver ist nicht erreichbar.");
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new StoerfallException(
                Stoerfall.Zeitueberschreitung,
                "Das technische Verbindungszeitlimit wurde überschritten.");
        }
    }

    private static async Task<Stream> OeffneStreamAsync(
        HttpContent inhalt,
        CancellationToken ct)
    {
        try
        {
            return await inhalt.ReadAsStreamAsync(ct);
        }
        catch (HttpRequestException)
        {
            throw new StoerfallException(
                Stoerfall.ModellserverNichtErreichbar,
                "Der Antwortstream des Modellservers konnte nicht geöffnet werden.");
        }
    }

    private static async Task<string?> LeseZeileAsync(
        StreamReader reader,
        CancellationToken ct)
    {
        try
        {
            return await reader.ReadLineAsync(ct);
        }
        catch (IOException)
        {
            throw new StoerfallException(
                Stoerfall.ModellserverNichtErreichbar,
                "Die Verbindung zum Antwortstream wurde unterbrochen.");
        }
    }

    private static IReadOnlyList<ChatNachricht> BaueVerlauf(
        IReadOnlyList<Nachricht> verlauf,
        string systemanweisung)
    {
        List<ChatNachricht> nachrichten = [new("system", systemanweisung)];

        foreach (Nachricht nachricht in verlauf)
        {
            nachrichten.Add(new ChatNachricht("user", nachricht.Text));

            if (nachricht.Antwort.Zustand == AntwortZustand.Fertig)
            {
                nachrichten.Add(new ChatNachricht("assistant", nachricht.Antwort.Text));
            }
        }

        return nachrichten;
    }

    private static SseEreignis LeseEreignis(string zeile)
    {
        if (!zeile.StartsWith("data:", StringComparison.Ordinal))
        {
            return SseEreignis.Leer;
        }

        string daten = zeile[5..].TrimStart();
        if (daten == "[DONE]")
        {
            return SseEreignis.Ende;
        }

        if (daten.Length == 0)
        {
            return SseEreignis.Leer;
        }

        try
        {
            using JsonDocument dokument = JsonDocument.Parse(daten);
            string? teil = dokument.RootElement
                .GetProperty("choices")[0]
                .GetProperty("delta")
                .TryGetProperty("content", out JsonElement inhalt)
                    ? inhalt.GetString()
                    : null;
            return new SseEreignis(teil, false);
        }
        catch (Exception ausnahme) when (
            ausnahme is JsonException or InvalidOperationException or KeyNotFoundException)
        {
            throw new StoerfallException(
                Stoerfall.ModellserverNichtErreichbar,
                "Der Modellserver hat einen ungültigen Antwortstream geliefert.");
        }
    }

    private static async Task<StoerfallException> ErzeugeStoerfallAsync(
        HttpResponseMessage antwort,
        CancellationToken ct)
    {
        string fehlerinhalt = await LeseFehlerinhaltAsync(antwort.Content, ct);
        bool modellUnbekannt = antwort.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.NotFound
            && EnthaeltUnbekanntesModell(fehlerinhalt);
        Stoerfall fall = modellUnbekannt
            ? Stoerfall.ModellUnbekannt
            : Stoerfall.ModellserverNichtErreichbar;

        return new StoerfallException(
            fall,
            $"Der Modellserver hat die Anfrage mit Status {(int)antwort.StatusCode} abgewiesen.");
    }

    private static async Task<string> LeseFehlerinhaltAsync(
        HttpContent inhalt,
        CancellationToken ct)
    {
        try
        {
            return await inhalt.ReadAsStringAsync(ct);
        }
        catch (HttpRequestException)
        {
            return string.Empty;
        }
    }

    private static bool EnthaeltUnbekanntesModell(string fehlerinhalt)
    {
        return fehlerinhalt.Contains("model", StringComparison.OrdinalIgnoreCase)
            && (fehlerinhalt.Contains("not found", StringComparison.OrdinalIgnoreCase)
                || fehlerinhalt.Contains("unknown", StringComparison.OrdinalIgnoreCase)
                || fehlerinhalt.Contains("does not exist", StringComparison.OrdinalIgnoreCase));
    }

    private sealed record ChatAnfrage(
        [property: JsonPropertyName("model")] string Modell,
        [property: JsonPropertyName("messages")] IReadOnlyList<ChatNachricht> Nachrichten,
        [property: JsonPropertyName("stream")] bool Stream,
        [property: JsonPropertyName("temperature")] double Temperatur,
        [property: JsonPropertyName("max_tokens")] int MaximaleAntwortlaenge);

    private sealed record ChatNachricht(
        [property: JsonPropertyName("role")] string Rolle,
        [property: JsonPropertyName("content")] string Inhalt);

    private sealed record SseEreignis(string? Teil, bool IstEnde)
    {
        public static SseEreignis Leer { get; } = new(null, false);

        public static SseEreignis Ende { get; } = new(null, true);
    }
}
