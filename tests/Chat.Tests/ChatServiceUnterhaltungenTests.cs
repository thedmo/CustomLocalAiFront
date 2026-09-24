using Chat.Core;
using Chat.Core.Modelle;
using Chat.Store.Sqlite;

namespace Chat.Tests;

public sealed class ChatServiceUnterhaltungenTests
{
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
