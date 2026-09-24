using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Chat.Store.Sqlite;

public sealed class ChatDbContextFactory : IDesignTimeDbContextFactory<ChatDbContext>
{
    public ChatDbContext CreateDbContext(string[] args)
    {
        // Migrationserzeugung benötigt das Modell, keine Datei mit Unterhaltungen.
        var options = new DbContextOptionsBuilder<ChatDbContext>()
            .UseSqlite("Data Source=:memory:").Options;
        return new ChatDbContext(options);
    }
}
