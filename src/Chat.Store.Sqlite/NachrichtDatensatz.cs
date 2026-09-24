namespace Chat.Store.Sqlite;

public sealed class NachrichtDatensatz
{
    public Guid Id { get; set; }
    public Guid UnterhaltungId { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTimeOffset Zeit { get; set; }
    public AntwortDatensatz Antwort { get; set; } = null!;
}
