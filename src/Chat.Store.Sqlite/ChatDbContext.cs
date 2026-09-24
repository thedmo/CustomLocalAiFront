using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Chat.Store.Sqlite;

public sealed class ChatDbContext(DbContextOptions<ChatDbContext> options) : DbContext(options)
{
    public DbSet<UnterhaltungDatensatz> Unterhaltungen => Set<UnterhaltungDatensatz>();
    public DbSet<NachrichtDatensatz> Nachrichten => Set<NachrichtDatensatz>();
    public DbSet<AntwortDatensatz> Antworten => Set<AntwortDatensatz>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ValueConverter<DateTimeOffset, string> zeit = new(
            wert => wert.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),
            wert => DateTimeOffset.Parse(wert, CultureInfo.InvariantCulture));
        var unterhaltung = modelBuilder.Entity<UnterhaltungDatensatz>();
        unterhaltung.ToTable("Unterhaltung");
        unterhaltung.HasKey(u => u.Id);
        unterhaltung.Property(u => u.Id).ValueGeneratedNever();
        unterhaltung.Property(u => u.ErstelltAm).HasConversion(zeit);
        unterhaltung.HasMany(u => u.Nachrichten).WithOne()
            .HasForeignKey(n => n.UnterhaltungId).OnDelete(DeleteBehavior.Cascade);

        var nachricht = modelBuilder.Entity<NachrichtDatensatz>();
        nachricht.ToTable("Nachricht");
        nachricht.HasKey(n => n.Id);
        nachricht.Property(n => n.Id).ValueGeneratedNever();
        nachricht.Property(n => n.Zeit).HasConversion(zeit);
        nachricht.HasOne(n => n.Antwort).WithOne()
            .HasForeignKey<AntwortDatensatz>(a => a.NachrichtId).OnDelete(DeleteBehavior.Cascade);

        var antwort = modelBuilder.Entity<AntwortDatensatz>();
        antwort.ToTable("Antwort");
        antwort.HasKey(a => a.Id);
        antwort.Property(a => a.Id).ValueGeneratedNever();
        antwort.Property(a => a.ErstelltAm).HasConversion(zeit);
    }
}
