using Chat.Core.Modelle;
using Chat.Store.Sqlite;

namespace Chat.Tests;

public sealed class SqliteNeustartTests
{
    [Fact]
    public async Task Neustart_FuenfUnterhaltungen_BewahrtTextUndEndzustaende()
    {
        CancellationToken ct = TestContext.Current.CancellationToken;
        await using SqliteTestdatenbank datei = await SqliteTestdatenbank.ErzeugeAsync();
        SqliteStore store = new(datei);
        Dictionary<Guid, AntwortZustand> erwartet = [];
        foreach (AntwortZustand zustand in Enum.GetValues<AntwortZustand>())
        {
            Antwort a = Antwort.Wiederherstellen(Guid.NewGuid(), "Teil", zustand, null, null, "Vorher");
            Unterhaltung u = new(Guid.NewGuid(), "Test", DateTimeOffset.UtcNow);
            u.FuegeNachrichtHinzu(new Nachricht(Guid.NewGuid(), "Test", u.ErstelltAm, a));
            await store.SpeichernAsync(u, ct);
            erwartet.Add(u.Id, zustand);
        }

        SqliteInitialisierung start = new(datei);
        await start.InitialisierenAsync(ct);
        await start.InitialisierenAsync(ct);
        SqliteStore neu = new(datei);
        Assert.Equal(5, (await neu.ListeAsync(ct)).Count);
        foreach ((Guid id, AntwortZustand vorher) in erwartet)
        {
            Antwort a = Assert.Single((await neu.LadenAsync(id, ct)).Nachrichten).Antwort;
            bool offen = vorher is AntwortZustand.Angefordert or AntwortZustand.Laeuft;
            Assert.Equal(offen ? AntwortZustand.Gestoert : vorher, a.Zustand);
            Assert.Equal(offen ? "Neustart" : "Vorher", a.Grund);
            Assert.Equal("Teil", a.Text);
        }
    }
}
