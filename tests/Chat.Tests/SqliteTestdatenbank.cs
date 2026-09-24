using Chat.Store.Sqlite;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Chat.Tests;

public sealed class SqliteTestdatenbank : IDbContextFactory<ChatDbContext>, IAsyncDisposable
{
    private readonly string _verzeichnis = Path.Combine(Path.GetTempPath(), "ChatTests-" + Guid.NewGuid());

    public string Dateipfad => Path.Combine(_verzeichnis, "test.db");
    public IInterceptor? Interceptor { get; set; }

    public static async Task<SqliteTestdatenbank> ErzeugeAsync()
    {
        SqliteTestdatenbank datenbank = new();
        Directory.CreateDirectory(datenbank._verzeichnis);
        await using ChatDbContext db = datenbank.CreateDbContext();
        await db.Database.MigrateAsync(TestContext.Current.CancellationToken);
        return datenbank;
    }

    public ChatDbContext CreateDbContext()
    {
        var verbindung = new SqliteConnectionStringBuilder { DataSource = Dateipfad, Pooling = false };
        var options = new DbContextOptionsBuilder<ChatDbContext>().UseSqlite(verbindung.ToString());
        if (Interceptor is not null)
        {
            options.AddInterceptors(Interceptor);
        }

        return new ChatDbContext(options.Options);
    }

    public ValueTask DisposeAsync()
    {
        // Ausschliesslich das für diese Instanz erzeugte Testverzeichnis entfernen.
        Directory.Delete(_verzeichnis, recursive: true);
        return ValueTask.CompletedTask;
    }
}
