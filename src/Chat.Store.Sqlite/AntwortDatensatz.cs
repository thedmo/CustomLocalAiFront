using Chat.Core;
using Chat.Core.Modelle;

namespace Chat.Store.Sqlite;

public sealed class AntwortDatensatz
{
    public Guid Id { get; set; }
    public Guid NachrichtId { get; set; }
    public string Text { get; set; } = string.Empty;
    public AntwortZustand Zustand { get; set; }
    public long? DauerMs { get; set; }
    public Stoerfall? Fall { get; set; }
    public string? Grund { get; set; }
    public DateTimeOffset ErstelltAm { get; set; }
}
