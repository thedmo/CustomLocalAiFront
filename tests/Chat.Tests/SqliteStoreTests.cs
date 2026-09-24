using Chat.Core;
using Chat.Core.Modelle;
using Chat.Store.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Chat.Tests;

public sealed class SqliteStoreTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Migration_LeereDatei_ErstelltSchemaUndIstWiederholbar()
    {
        await using SqliteTestdatenbank datei = await SqliteTestdatenbank.ErzeugeAsync();
        await using ChatDbContext db = datei.CreateDbContext();
        await db.Database.MigrateAsync(Ct);
        Assert.Single(await db.Database.GetAppliedMigrationsAsync(Ct));
        Assert.False(db.Database.HasPendingModelChanges());
        Assert.Empty(await new SqliteStore(datei).ListeAsync(Ct));
        Assert.Equal(0, await db.Nachrichten.CountAsync(Ct));
        Assert.Equal(0, await db.Antworten.CountAsync(Ct));
    }

    [Fact]
    public async Task Speichern_LeereUnterhaltung_BleibtNachNeuoeffnenErhalten()
    {
        await using SqliteTestdatenbank datei = await SqliteTestdatenbank.ErzeugeAsync();
        Unterhaltung original = new(Guid.NewGuid(), "Test", DateTimeOffset.UtcNow);
        await new SqliteStore(datei).SpeichernAsync(original, Ct);
        Unterhaltung geladen = await new SqliteStore(datei).LadenAsync(original.Id, Ct);
        Assert.Equal(original.Id, geladen.Id);
        Assert.Equal(original.Titel, geladen.Titel);
        Assert.Equal(original.ErstelltAm, geladen.ErstelltAm);
        Assert.Empty(geladen.Nachrichten);
    }

    [Theory]
    [InlineData(AntwortZustand.Angefordert)]
    [InlineData(AntwortZustand.Laeuft)]
    [InlineData(AntwortZustand.Fertig)]
    [InlineData(AntwortZustand.Abgebrochen)]
    [InlineData(AntwortZustand.Gestoert)]
    public async Task Laden_Verlaeufe_BehaeltZuordnungUndZustaende(AntwortZustand zustand)
    {
        await using SqliteTestdatenbank datei = await SqliteTestdatenbank.ErzeugeAsync();
        DateTimeOffset zeit = DateTimeOffset.UtcNow;
        Antwort antwort = Antwort.Wiederherstellen(Guid.NewGuid(), "Teil", zustand,
            TimeSpan.FromMilliseconds(123), Stoerfall.Zeitueberschreitung, "Testgrund");
        Nachricht spaet = new(Guid.NewGuid(), "Zwei", zeit.AddMinutes(1), antwort);
        Nachricht frueh = new(Guid.NewGuid(), "Eins", zeit, new Antwort(Guid.NewGuid()));
        Unterhaltung original = Unterhaltung.Wiederherstellen(Guid.NewGuid(), "Titel", zeit, [spaet, frueh]);
        SqliteStore store = new(datei);
        await store.SpeichernAsync(original, Ct);
        await store.SpeichernAsync(original, Ct);
        Unterhaltung geladen = await new SqliteStore(datei).LadenAsync(original.Id, Ct);
        Assert.Equal("Titel", geladen.Titel);
        Assert.Equal(new[] { frueh.Id, spaet.Id }, geladen.Nachrichten.Select(n => n.Id));
        Antwort a = geladen.Nachrichten[1].Antwort;
        Assert.Equal(antwort.Id, a.Id);
        Assert.Equal(antwort.Text, a.Text);
        Assert.Equal(zustand, a.Zustand);
        Assert.Equal(antwort.Dauer, a.Dauer);
        Assert.Equal(antwort.Fall, a.Fall);
        Assert.Equal(antwort.Grund, a.Grund);
        await using ChatDbContext db = datei.CreateDbContext();
        Assert.Equal(2, await db.Nachrichten.CountAsync(Ct));
        Assert.Equal(2, await db.Antworten.CountAsync(Ct));
        Assert.Equal(spaet.Zeit, (await db.Antworten.SingleAsync(x => x.Id == a.Id, Ct)).ErstelltAm);
    }

    [Fact]
    public async Task Speichern_AenderungWirdErstBeimNaechstenAufrufDauerhaft()
    {
        await using SqliteTestdatenbank datei = await SqliteTestdatenbank.ErzeugeAsync();
        SqliteStore store = new(datei);
        Unterhaltung u = new(Guid.NewGuid(), "Leer", DateTimeOffset.UtcNow);
        await store.SpeichernAsync(u, Ct);
        Antwort a = new(Guid.NewGuid());
        u.FuegeNachrichtHinzu(new Nachricht(Guid.NewGuid(), "Test", DateTimeOffset.UtcNow, a));
        Assert.Empty((await store.LadenAsync(u.Id, Ct)).Nachrichten);
        await store.SpeichernAsync(u, Ct);
        a.WechsleZu(AntwortZustand.Laeuft);
        a.FuegeTeilHinzu("Teil");
        await store.SpeichernAsync(u, Ct);
        Unterhaltung geladen = await store.LadenAsync(u.Id, Ct);
        Assert.Equal("Test", geladen.Titel);
        Assert.Equal("Teil", Assert.Single(geladen.Nachrichten).Antwort.Text);
    }

    [Fact]
    public async Task Liste_Zeitpunkte_SindStabilSortiert()
    {
        await using SqliteTestdatenbank datei = await SqliteTestdatenbank.ErzeugeAsync();
        SqliteStore store = new(datei);
        DateTimeOffset zeit = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
        Unterhaltung a = new(Guid.Parse("00000000-0000-0000-0000-000000000001"), "A", zeit);
        Unterhaltung b = new(Guid.Parse("00000000-0000-0000-0000-000000000002"), "B", zeit.ToOffset(TimeSpan.FromHours(2)));
        Unterhaltung c = new(Guid.NewGuid(), "C", zeit.AddMinutes(1));
        foreach (Unterhaltung u in new[] { b, a, c })
        {
            await store.SpeichernAsync(u, Ct);
        }

        Assert.Equal(new[] { c.Id, a.Id, b.Id }, (await store.ListeAsync(Ct)).Select(u => u.Id));
    }

    [Fact]
    public async Task Laden_UnbekannteKennung_UndCancellationWerdenWeitergegeben()
    {
        await using SqliteTestdatenbank datei = await SqliteTestdatenbank.ErzeugeAsync();
        SqliteStore store = new(datei);
        await Assert.ThrowsAsync<KeyNotFoundException>(() => store.LadenAsync(Guid.NewGuid(), Ct));
        using CancellationTokenSource abbruch = new();
        abbruch.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => store.ListeAsync(abbruch.Token));
    }

    [Fact]
    public async Task Speichern_Fehlgeschlagen_RolltTransaktionZurueck()
    {
        await using SqliteTestdatenbank datei = await SqliteTestdatenbank.ErzeugeAsync();
        SqliteStore store = new(datei);
        Unterhaltung u = new(Guid.NewGuid(), "Vorher", DateTimeOffset.UtcNow);
        await store.SpeichernAsync(u, Ct);
        u.FuegeNachrichtHinzu(new Nachricht(Guid.NewGuid(), "Nachher", DateTimeOffset.UtcNow, new Antwort(Guid.NewGuid())));
        datei.Interceptor = new FehlerNachSpeichern();
        await Assert.ThrowsAsync<InvalidOperationException>(() => store.SpeichernAsync(u, Ct));
        datei.Interceptor = null;
        Unterhaltung geladen = await store.LadenAsync(u.Id, Ct);
        Assert.Equal("Vorher", geladen.Titel);
        Assert.Empty(geladen.Nachrichten);
    }

    private sealed class FehlerNachSpeichern : SaveChangesInterceptor
    {
        public override ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
        {
            // SQL wurde ausgeführt, die explizite Store-Transaktion aber noch nicht bestätigt.
            throw new InvalidOperationException("Simulierter Transaktionsfehler");
        }
    }
}
