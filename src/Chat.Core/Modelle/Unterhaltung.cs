namespace Chat.Core.Modelle;

public sealed class Unterhaltung
{
    private readonly List<Nachricht> _nachrichten = [];

    public Unterhaltung(Guid id, string titel, DateTimeOffset erstelltAm)
    {
        Id = id;
        Titel = titel;
        ErstelltAm = erstelltAm;
    }

    public Guid Id { get; }

    public string Titel { get; private set; }

    public DateTimeOffset ErstelltAm { get; }

    public IReadOnlyList<Nachricht> Nachrichten => _nachrichten;

    /// <summary>Übernimmt den gespeicherten Titel unabhängig von der Nachrichtenreihenfolge.</summary>
    public static Unterhaltung Wiederherstellen(
        Guid id, string titel, DateTimeOffset erstelltAm, IEnumerable<Nachricht> nachrichten)
    {
        Unterhaltung unterhaltung = new(id, titel, erstelltAm);
        unterhaltung._nachrichten.AddRange(nachrichten);
        return unterhaltung;
    }

    public void FuegeNachrichtHinzu(Nachricht nachricht)
    {
        ArgumentNullException.ThrowIfNull(nachricht);
        _nachrichten.Add(nachricht);

        if (_nachrichten.Count == 1)
        {
            Titel = nachricht.Text.Length <= 50
                ? nachricht.Text
                : nachricht.Text[..50];
        }
    }
}
