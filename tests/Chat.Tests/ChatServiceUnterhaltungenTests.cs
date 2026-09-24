using Chat.Core;
using Chat.Core.Modelle;
using Chat.Store.Sqlite;

namespace Chat.Tests;

public sealed class ChatServiceUnterhaltungenTests
{
    [Fact]
    public async Task Loeschen_OhneModellUndMitUnbekannterKennung_IstWiederholbar()
    {
        CancellationToken ct = TestContext.Current.CancellationToken;
        ArbeitsspeicherStore store = new();
        FakeModelServerClient client = new();
        ChatService service = new(client, store, new Konfiguration());
        Unterhaltung zuLoeschen = new(Guid.NewGuid(), "Löschen", DateTimeOffset.UtcNow);
        Unterhaltung zuBehalten = new(Guid.NewGuid(), "Behalten", DateTimeOffset.UtcNow.AddSeconds(1));
        await store.SpeichernAsync(zuLoeschen, ct);
        await store.SpeichernAsync(zuBehalten, ct);
        Guid id = zuLoeschen.Id;
        await service.LoescheUnterhaltungAsync(id, ct);
        await service.LoescheUnterhaltungAsync(id, ct);
        Assert.Equal(zuBehalten.Id, Assert.Single(await service.ListeUnterhaltungenAsync(ct)).Id);
        await Assert.ThrowsAsync<KeyNotFoundException>(() => store.LadenAsync(id, ct));
        Assert.Equal(0, client.Aufrufe);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Loeschen_AktiveAntwortInAndererSitzung_ErstNachEndeMoeglich(bool abbrechen)
    {
        CancellationToken ct = TestContext.Current.CancellationToken;
        SpeicherStore store = new();
        ChatService sendend = ErzeugeService(store);
        ChatService loeschend = ErzeugeService(store);
        Guid id = await sendend.NeueUnterhaltungAsync(ct);
        AntwortLauf lauf = await sendend.SendeNachrichtAsync(id, "Test", ct);
        await Assert.ThrowsAsync<InvalidOperationException>(() => loeschend.LoescheUnterhaltungAsync(id, ct));
        await using IAsyncEnumerator<string> stream = lauf.Teile.GetAsyncEnumerator(ct);
        Assert.True(await stream.MoveNextAsync());
        await Assert.ThrowsAsync<InvalidOperationException>(() => loeschend.LoescheUnterhaltungAsync(id, ct));
        if (abbrechen)
        {
            await sendend.AbbrechenAsync(lauf.AntwortId, ct);
        }

        Assert.False(await stream.MoveNextAsync());
        await loeschend.LoescheUnterhaltungAsync(id, ct);
        Assert.Empty(await store.ListeAsync(ct));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SendenUndLoeschen_Gleichzeitig_StelltKeineDatenWiederHer(bool sendenZuerst)
    {
        CancellationToken ct = TestContext.Current.CancellationToken;
        GesteuerterStore store = new();
        ChatService sendend = ErzeugeService(store);
        ChatService loeschend = ErzeugeService(store);
        Unterhaltung unterhaltung = new(Guid.NewGuid(), "Test", DateTimeOffset.UtcNow);
        store.FuegeHinzu(unterhaltung);
        Guid id = unterhaltung.Id;
        store.BlockiereSpeichern = sendenZuerst;
        store.BlockiereLoeschen = !sendenZuerst;
        Task? loeschen = sendenZuerst ? null : loeschend.LoescheUnterhaltungAsync(id, ct);
        Task<AntwortLauf>? senden = sendenZuerst ? sendend.SendeNachrichtAsync(id, "Test", ct) : null;
        await store.Gestartet.Task.WaitAsync(ct);
        loeschen ??= loeschend.LoescheUnterhaltungAsync(id, ct);
        senden ??= sendend.SendeNachrichtAsync(id, "Test", ct);
        Assert.False(loeschen.IsCompleted);
        Assert.False(senden.IsCompleted);
        store.Freigabe.SetResult();
        if (sendenZuerst)
        {
            AntwortLauf lauf = await senden;
            await Assert.ThrowsAsync<InvalidOperationException>(() => loeschen);
            await sendend.AbbrechenAsync(lauf.AntwortId, ct);
            await loeschend.LoescheUnterhaltungAsync(id, ct);
        }
        else
        {
            await loeschen;
            await Assert.ThrowsAsync<KeyNotFoundException>(() => senden);
        }
        Assert.Empty(await store.ListeAsync(ct));
    }

    [Fact]
    public async Task Loeschen_FehlerUndCancellation_GebenSperreFreiUndErhaltenDaten()
    {
        CancellationToken ct = TestContext.Current.CancellationToken;
        GesteuerterStore store = new();
        ChatService service = ErzeugeService(store);
        Unterhaltung unterhaltung = new(Guid.NewGuid(), "Test", DateTimeOffset.UtcNow);
        store.FuegeHinzu(unterhaltung);
        Guid id = unterhaltung.Id;
        store.LoeschFehler = new IOException("Simulierter Speicherfehler");
        await Assert.ThrowsAsync<IOException>(() => service.LoescheUnterhaltungAsync(id, ct));
        Assert.Single(await store.ListeAsync(ct));
        store.LoeschFehler = null;
        using CancellationTokenSource abbruch = new();
        abbruch.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.LoescheUnterhaltungAsync(id, abbruch.Token));
        Assert.Single(await store.ListeAsync(ct));
        await service.LoescheUnterhaltungAsync(id, ct);
        Assert.Empty(await store.ListeAsync(ct));
    }

    private static ChatService ErzeugeService(IStore store) => new(new FakeModelServerClient(), store,
        new Konfiguration { AdresseModellserver = "http://model-runner.invalid/engines/v1/", Modellname = "test" });

    private sealed class GesteuerterStore : IStore
    {
        private readonly SpeicherStore _inner = new();
        public bool BlockiereSpeichern { get; set; }
        public bool BlockiereLoeschen { get; set; }
        public Exception? LoeschFehler { get; set; }
        public TaskCompletionSource Gestartet { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Freigabe { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public void FuegeHinzu(Unterhaltung unterhaltung) => _inner.FuegeHinzu(unterhaltung);
        public Task<IReadOnlyList<UnterhaltungInfo>> ListeAsync(CancellationToken ct) => _inner.ListeAsync(ct);
        public Task<Unterhaltung> LadenAsync(Guid id, CancellationToken ct) => _inner.LadenAsync(id, ct);
        public async Task SpeichernAsync(Unterhaltung unterhaltung, CancellationToken ct)
        {
            if (BlockiereSpeichern)
            {
                await WarteAsync(ct);
            }

            await _inner.SpeichernAsync(unterhaltung, ct);
        }
        public async Task LoeschenAsync(Guid id, CancellationToken ct)
        {
            if (LoeschFehler is not null)
            {
                throw LoeschFehler;
            }

            if (BlockiereLoeschen)
            {
                await WarteAsync(ct);
            }

            await _inner.LoeschenAsync(id, ct);
        }
        private async Task WarteAsync(CancellationToken ct)
        {
            Gestartet.TrySetResult();
            await Freigabe.Task.WaitAsync(ct);
        }
    }

    [Fact]
    public async Task GeoeffneteUnterhaltung_WeitereNachricht_ErhaeltBisherigenVerlauf()
    {
        CancellationToken ct = TestContext.Current.CancellationToken;
        await using SqliteTestdatenbank datei = await SqliteTestdatenbank.ErzeugeAsync();
        Konfiguration konfiguration = new()
        {
            AdresseModellserver = "http://model-runner.invalid/engines/v1/",
            Modellname = "test"
        };
        ChatService zuerst = new(new FakeModelServerClient(), new SqliteStore(datei), konfiguration);
        Guid id = await zuerst.NeueUnterhaltungAsync(ct);
        AntwortLauf erste = await zuerst.SendeNachrichtAsync(id, "Erste Frage", ct);
        await foreach (string teil in erste.Teile.WithCancellation(ct))
        {
            Assert.NotEmpty(teil);
        }

        ChatService neu = new(new FakeModelServerClient(), new SqliteStore(datei), konfiguration);
        Unterhaltung vorher = await neu.OeffneUnterhaltungAsync(id, ct);
        AntwortLauf zweite = await neu.SendeNachrichtAsync(vorher.Id, "Zweite Frage", ct);
        await foreach (string teil in zweite.Teile.WithCancellation(ct))
        {
            Assert.NotEmpty(teil);
        }

        Unterhaltung nachher = await neu.OeffneUnterhaltungAsync(id, ct);
        Assert.Equal(2, nachher.Nachrichten.Count);
        Assert.Equal(vorher.Nachrichten[0].Id, nachher.Nachrichten[0].Id);
        Assert.Equal(vorher.Nachrichten[0].Antwort.Text, nachher.Nachrichten[0].Antwort.Text);
        Assert.Equal("Erste Frage", nachher.Titel);
        Assert.Equal("Zweite Frage", nachher.Nachrichten[1].Text);
        Assert.All(nachher.Nachrichten, n => Assert.Equal(AntwortZustand.Fertig, n.Antwort.Zustand));
    }

    [Fact]
    public async Task ListeUndOeffnen_NachNeustart_LaedtFuenfVerlaeufeOhneModell()
    {
        CancellationToken ct = TestContext.Current.CancellationToken;
        await using SqliteTestdatenbank datei = await SqliteTestdatenbank.ErzeugeAsync();
        SqliteStore store = new(datei);
        List<Unterhaltung> original = [];
        for (int i = 0; i < 5; i++)
        {
            Unterhaltung u = new(Guid.NewGuid(), "Test", DateTimeOffset.UtcNow.AddMinutes(i));
            if (i > 0)
            {
                u.FuegeNachrichtHinzu(new Nachricht(Guid.NewGuid(), "Zwei", u.ErstelltAm.AddSeconds(1),
                    Antwort.Wiederherstellen(Guid.NewGuid(), "Antwort zwei", AntwortZustand.Abgebrochen, null, null, null)));
                u.FuegeNachrichtHinzu(new Nachricht(Guid.NewGuid(), "Eins", u.ErstelltAm,
                    Antwort.Wiederherstellen(Guid.NewGuid(), "Antwort eins", AntwortZustand.Fertig, null, null, null)));
            }
            original.Add(u);
            await store.SpeichernAsync(u, ct);
        }

        await new SqliteInitialisierung(datei).InitialisierenAsync(ct);
        FakeModelServerClient client = new() { Ausnahme = new StoerfallException(Stoerfall.ModellserverNichtErreichbar, "Test") };
        // Ungültige Modellkonfiguration darf reine Leseoperationen ebenfalls nicht verhindern.
        ChatService service = new(client, new SqliteStore(datei), new Konfiguration());
        Assert.Equal(original.AsEnumerable().Reverse().Select(u => u.Id),
            (await service.ListeUnterhaltungenAsync(ct)).Select(u => u.Id));
        foreach (Unterhaltung erwartet in original)
        {
            Unterhaltung geladen = await service.OeffneUnterhaltungAsync(erwartet.Id, ct);
            Assert.Equal(erwartet.Id, geladen.Id);
            Assert.Equal(erwartet.Nachrichten.OrderBy(n => n.Zeit).Select(n => (n.Id, n.Antwort.Id, n.Antwort.Text, n.Antwort.Zustand)),
                geladen.Nachrichten.Select(n => (n.Id, n.Antwort.Id, n.Antwort.Text, n.Antwort.Zustand)));
        }

        Assert.Equal(0, client.Aufrufe);
    }

    [Fact]
    public async Task ListeUndOeffnen_LeerUnbekanntUndAbgebrochen()
    {
        CancellationToken ct = TestContext.Current.CancellationToken;
        await using SqliteTestdatenbank datei = await SqliteTestdatenbank.ErzeugeAsync();
        ChatService service = new(new FakeModelServerClient(), new SqliteStore(datei), new Konfiguration());
        Assert.Empty(await service.ListeUnterhaltungenAsync(ct));
        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.OeffneUnterhaltungAsync(Guid.NewGuid(), ct));
        using CancellationTokenSource abbruch = new();
        abbruch.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.ListeUnterhaltungenAsync(abbruch.Token));
    }
}
