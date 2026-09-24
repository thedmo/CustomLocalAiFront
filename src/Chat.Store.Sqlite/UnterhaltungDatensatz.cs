namespace Chat.Store.Sqlite;

public sealed class UnterhaltungDatensatz
{
    public Guid Id { get; set; }
    public string Titel { get; set; } = string.Empty;
    public DateTimeOffset ErstelltAm { get; set; }
    public List<NachrichtDatensatz> Nachrichten { get; set; } = [];
}
