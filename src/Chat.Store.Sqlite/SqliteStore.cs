using Chat.Core;
using Chat.Core.Modelle;
using Microsoft.EntityFrameworkCore;

namespace Chat.Store.Sqlite;

public sealed class SqliteStore(IDbContextFactory<ChatDbContext> factory) : IStore
{
    public async Task<IReadOnlyList<UnterhaltungInfo>> ListeAsync(CancellationToken ct)
    {
        await using ChatDbContext db = await factory.CreateDbContextAsync(ct);
        var liste = await db.Unterhaltungen.AsNoTracking()
            .Select(u => new UnterhaltungInfo(u.Id, u.Titel, u.ErstelltAm)).ToListAsync(ct);
        return liste.OrderByDescending(u => u.ErstelltAm).ThenBy(u => u.Id).ToArray();
    }

    public async Task<Unterhaltung> LadenAsync(Guid id, CancellationToken ct)
    {
        await using ChatDbContext db = await factory.CreateDbContextAsync(ct);
        UnterhaltungDatensatz daten = await db.Unterhaltungen.AsNoTracking()
            .Include(u => u.Nachrichten).ThenInclude(n => n.Antwort)
            .SingleOrDefaultAsync(u => u.Id == id, ct)
            ?? throw new KeyNotFoundException("Die Unterhaltung wurde nicht gefunden.");
        return Unterhaltung.Wiederherstellen(daten.Id, daten.Titel, daten.ErstelltAm,
            daten.Nachrichten.OrderBy(n => n.Zeit).ThenBy(n => n.Id).Select(LadeNachricht));
    }

    public async Task SpeichernAsync(Unterhaltung unterhaltung, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(unterhaltung);
        await using ChatDbContext db = await factory.CreateDbContextAsync(ct);
        await using var transaktion = await db.Database.BeginTransactionAsync(ct);
        UnterhaltungDatensatz? daten = await db.Unterhaltungen
            .Include(u => u.Nachrichten).ThenInclude(n => n.Antwort)
            .SingleOrDefaultAsync(u => u.Id == unterhaltung.Id, ct);
        if (daten is null)
        {
            daten = new UnterhaltungDatensatz
            {
                Id = unterhaltung.Id,
                ErstelltAm = unterhaltung.ErstelltAm
            };
            db.Unterhaltungen.Add(daten);
        }

        daten.Titel = unterhaltung.Titel;
        foreach (Nachricht nachricht in unterhaltung.Nachrichten)
        {
            AktualisiereNachricht(daten, nachricht);
        }

        await db.SaveChangesAsync(ct);
        await transaktion.CommitAsync(ct);
    }

    private static Nachricht LadeNachricht(NachrichtDatensatz daten)
    {
        AntwortDatensatz a = daten.Antwort;
        Antwort antwort = Antwort.Wiederherstellen(a.Id, a.Text, a.Zustand,
            a.DauerMs is long dauer ? TimeSpan.FromMilliseconds(dauer) : null, a.Fall, a.Grund);
        return new Nachricht(daten.Id, daten.Text, daten.Zeit, antwort);
    }

    private static void AktualisiereNachricht(UnterhaltungDatensatz daten, Nachricht nachricht)
    {
        NachrichtDatensatz? gespeichert = daten.Nachrichten.SingleOrDefault(n => n.Id == nachricht.Id);
        if (gespeichert is null)
        {
            gespeichert = new NachrichtDatensatz
            {
                Id = nachricht.Id,
                UnterhaltungId = daten.Id,
                Text = nachricht.Text,
                Zeit = nachricht.Zeit,
                Antwort = new AntwortDatensatz
                {
                    Id = nachricht.Antwort.Id,
                    NachrichtId = nachricht.Id,
                    ErstelltAm = nachricht.Zeit
                }
            };
            daten.Nachrichten.Add(gespeichert);
        }

        Antwort antwort = nachricht.Antwort;
        gespeichert.Antwort.Text = antwort.Text;
        gespeichert.Antwort.Zustand = antwort.Zustand;
        gespeichert.Antwort.DauerMs = antwort.Dauer is TimeSpan dauer ? (long)dauer.TotalMilliseconds : null;
        gespeichert.Antwort.Fall = antwort.Fall;
        gespeichert.Antwort.Grund = antwort.Grund;
    }
}
