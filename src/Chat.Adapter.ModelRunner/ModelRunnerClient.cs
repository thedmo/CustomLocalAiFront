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
            throw ErzeugeStoerfall(antwort.StatusCode);
        }

        await using Stream stream = await antwort.Content.ReadAsStreamAsync(ct);
        using StreamReader reader = new(stream);

        while (await reader.ReadLineAsync(ct) is { } zeile)
        {
            string? teil = LeseTeil(zeile);
            if (!string.IsNullOrEmpty(teil))
            {
                yield return teil;
            }
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

    private static string? LeseTeil(string zeile)
    {
        if (!zeile.StartsWith("data:", StringComparison.Ordinal))
        {
            return null;
        }

        string daten = zeile[5..].TrimStart();
        if (daten.Length == 0 || daten == "[DONE]")
        {
            return null;
        }

        try
        {
            using JsonDocument dokument = JsonDocument.Parse(daten);
            return dokument.RootElement
                .GetProperty("choices")[0]
                .GetProperty("delta")
                .TryGetProperty("content", out JsonElement inhalt)
                    ? inhalt.GetString()
                    : null;
        }
        catch (Exception ausnahme) when (
            ausnahme is JsonException or InvalidOperationException or KeyNotFoundException)
        {
            throw new StoerfallException(
                Stoerfall.ModellserverNichtErreichbar,
                "Der Modellserver hat einen ungültigen Antwortstream geliefert.");
        }
    }

    private static StoerfallException ErzeugeStoerfall(HttpStatusCode status)
    {
        Stoerfall fall = status is HttpStatusCode.BadRequest or HttpStatusCode.NotFound
            ? Stoerfall.ModellUnbekannt
            : Stoerfall.ModellserverNichtErreichbar;

        return new StoerfallException(
            fall,
            $"Der Modellserver hat die Anfrage mit Status {(int)status} abgewiesen.");
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
}
