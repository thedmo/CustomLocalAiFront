using Chat.Core.Modelle;
using Microsoft.EntityFrameworkCore;

namespace Chat.Store.Sqlite;

public sealed class SqliteInitialisierung(IDbContextFactory<ChatDbContext> factory)
{
    public async Task InitialisierenAsync(CancellationToken ct)
    {
        await using ChatDbContext db = await factory.CreateDbContextAsync(ct);
        await db.Database.MigrateAsync(ct);
        // Nur beim Prozessstart ausführen: Ein neuer Circuit ist kein Neustart.
        await db.Antworten
            .Where(a => a.Zustand == AntwortZustand.Angefordert || a.Zustand == AntwortZustand.Laeuft)
            .ExecuteUpdateAsync(update => update
                .SetProperty(a => a.Zustand, AntwortZustand.Gestoert)
                .SetProperty(a => a.Grund, "Neustart")
                .SetProperty(a => a.Fall, (Chat.Core.Stoerfall?)null), ct);
    }
}
