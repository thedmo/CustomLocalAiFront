using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Chat.Core.Modelle;

namespace Chat.Core;

public sealed class ChatService : IChatService
{
    private readonly ConcurrentDictionary<Guid, AktiveAnfrage> _aktiveAnfragen = new();
    private readonly IModelServerClient _client;
    private readonly IStore _store;
    private readonly Konfiguration _konfiguration;

    public ChatService(
        IModelServerClient client,
        IStore store,
        Konfiguration konfiguration)
    {
        _client = client;
        _store = store;
        _konfiguration = konfiguration;
    }

    public async Task<Guid> NeueUnterhaltungAsync(CancellationToken ct)
    {
        Unterhaltung unterhaltung = new(
            Guid.NewGuid(),
            "Neue Unterhaltung",
            DateTimeOffset.UtcNow);
        await _store.SpeichernAsync(unterhaltung, ct);
        return unterhaltung.Id;
    }

    public async Task<AntwortLauf> SendeNachrichtAsync(
        Guid unterhaltungId,
        string text,
        CancellationToken ct)
    {
        PruefeEingabe(text);
        _konfiguration.Pruefen();

        Unterhaltung unterhaltung = await _store.LadenAsync(unterhaltungId, ct);
        Antwort antwort = new(Guid.NewGuid());
        Nachricht nachricht = new(Guid.NewGuid(), text, DateTimeOffset.UtcNow, antwort);
        unterhaltung.FuegeNachrichtHinzu(nachricht);
        await _store.SpeichernAsync(unterhaltung, ct);

        AktiveAnfrage anfrage = new(unterhaltung, antwort, _konfiguration.ZeitlimitSekunden, ct);
        if (!_aktiveAnfragen.TryAdd(antwort.Id, anfrage))
        {
            anfrage.Dispose();
            throw new InvalidOperationException("Die Antwort-ID ist bereits aktiv.");
        }

        _ = BeobachteLebenszyklusAsync(anfrage);

        return new AntwortLauf(antwort.Id, StreameAntwortAsync(anfrage));
    }

    public async Task AbbrechenAsync(Guid antwortId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (!_aktiveAnfragen.TryGetValue(antwortId, out AktiveAnfrage? anfrage))
        {
            return;
        }

        anfrage.BenutzerAbbruch.Cancel();
        await SchliesseAsync(anfrage, AntwortZustand.Abgebrochen, null, null);
    }

    private async IAsyncEnumerable<string> StreameAntwortAsync(
        AktiveAnfrage anfrage,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        if (anfrage.IstAbgeschlossen)
        {
            yield break;
        }

        using CancellationTokenSource aufzaehlung =
            CancellationTokenSource.CreateLinkedTokenSource(anfrage.Token, ct);
        await using IAsyncEnumerator<string> enumerator = _client
            .StreamAntwortAsync(
                anfrage.Unterhaltung.Nachrichten,
                _konfiguration.Systemanweisung,
                aufzaehlung.Token)
            .GetAsyncEnumerator(aufzaehlung.Token);

        bool hatTeil = false;
        while (true)
        {
            StreamSchritt schritt = await LeseNaechstenSchrittAsync(
                enumerator,
                anfrage,
                ct);
            if (schritt.Ausnahme is not null)
            {
                throw schritt.Ausnahme;
            }

            if (schritt.Abgebrochen || schritt.IstEnde)
            {
                break;
            }

            if (!anfrage.VersucheTeilHinzuzufuegen(schritt.Teil!))
            {
                yield break;
            }

            hatTeil = true;
            yield return schritt.Teil!;
        }

        if (anfrage.IstAbgeschlossen)
        {
            yield break;
        }

        if (!hatTeil)
        {
            StoerfallException ausnahme = new(
                Stoerfall.ModellserverNichtErreichbar,
                "Der Modellserver hat keine Antwortteile geliefert.");
            await SchliesseGestoertAsync(anfrage, ausnahme);
            throw ausnahme;
        }

        await SchliesseAsync(anfrage, AntwortZustand.Fertig, null, null);
    }

    private async Task<StreamSchritt> LeseNaechstenSchrittAsync(
        IAsyncEnumerator<string> enumerator,
        AktiveAnfrage anfrage,
        CancellationToken aufzaehlung)
    {
        try
        {
            bool hatNaechsten = await enumerator.MoveNextAsync();
            return hatNaechsten
                ? StreamSchritt.MitTeil(enumerator.Current)
                : StreamSchritt.Ende;
        }
        catch (OperationCanceledException) when (anfrage.BenutzerAbbruch.IsCancellationRequested)
        {
            await SchliesseAsync(anfrage, AntwortZustand.Abgebrochen, null, null);
            return StreamSchritt.Abbruch;
        }
        catch (OperationCanceledException) when (anfrage.Zeitlimit.IsCancellationRequested)
        {
            StoerfallException ausnahme = new(
                Stoerfall.Zeitueberschreitung,
                "Das Zeitlimit für die Modellantwort wurde überschritten.");
            await SchliesseGestoertAsync(anfrage, ausnahme);
            return StreamSchritt.MitAusnahme(ausnahme);
        }
        catch (OperationCanceledException) when (
            anfrage.Lebenszyklus.IsCancellationRequested || aufzaehlung.IsCancellationRequested)
        {
            StoerfallException ausnahme = ErzeugeVerbindungsverlust();
            await SchliesseGestoertAsync(anfrage, ausnahme);
            return StreamSchritt.MitAusnahme(ausnahme);
        }
        catch (OperationCanceledException)
        {
            StoerfallException ausnahme = new(
                Stoerfall.Zeitueberschreitung,
                "Die Modellanfrage wurde durch ein technisches Zeitlimit beendet.");
            await SchliesseGestoertAsync(anfrage, ausnahme);
            return StreamSchritt.MitAusnahme(ausnahme);
        }
        catch (StoerfallException ausnahme)
        {
            await SchliesseGestoertAsync(anfrage, ausnahme);
            return StreamSchritt.MitAusnahme(ausnahme);
        }
    }

    private Task SchliesseGestoertAsync(AktiveAnfrage anfrage, StoerfallException ausnahme)
    {
        return SchliesseAsync(
            anfrage,
            AntwortZustand.Gestoert,
            ausnahme.Fall,
            ausnahme.Message);
    }

    private async Task BeobachteLebenszyklusAsync(AktiveAnfrage anfrage)
    {
        try
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, anfrage.BeobachtungToken);
        }
        catch (OperationCanceledException) when (anfrage.Lebenszyklus.IsCancellationRequested)
        {
            await SchliesseGestoertAsync(anfrage, ErzeugeVerbindungsverlust());
        }
        catch (OperationCanceledException) when (anfrage.IstAbgeschlossen)
        {
            // Ein fachlicher Endzustand beendet nur die interne Beobachtung.
        }
    }

    private static StoerfallException ErzeugeVerbindungsverlust()
    {
        return new StoerfallException(
            Stoerfall.ModellserverNichtErreichbar,
            "Die Verbindung zur Chat-Seite wurde beendet.");
    }

    private async Task SchliesseAsync(
        AktiveAnfrage anfrage,
        AntwortZustand zustand,
        Stoerfall? fall,
        string? grund)
    {
        if (!anfrage.VersucheAbzuschliessen(zustand, fall, grund))
        {
            return;
        }

        await _store.SpeichernAsync(anfrage.Unterhaltung, CancellationToken.None);
        _aktiveAnfragen.TryRemove(anfrage.Antwort.Id, out _);
        anfrage.Dispose();
    }

    private void PruefeEingabe(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new StoerfallException(
                Stoerfall.KonfigurationUngueltig,
                "Die Nachricht darf nicht leer sein.");
        }

        if (text.Length > _konfiguration.Eingabegrenze)
        {
            throw new StoerfallException(
                Stoerfall.KonfigurationUngueltig,
                $"Die Nachricht überschreitet die Eingabegrenze von {_konfiguration.Eingabegrenze} Zeichen.");
        }
    }

    private sealed record StreamSchritt(
        string? Teil,
        bool IstEnde,
        bool Abgebrochen,
        StoerfallException? Ausnahme)
    {
        public static StreamSchritt Ende { get; } = new(null, true, false, null);

        public static StreamSchritt Abbruch { get; } = new(null, false, true, null);

        public static StreamSchritt MitTeil(string teil) => new(teil, false, false, null);

        public static StreamSchritt MitAusnahme(StoerfallException ausnahme) =>
            new(null, false, false, ausnahme);
    }
}
