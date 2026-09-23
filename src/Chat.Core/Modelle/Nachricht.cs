namespace Chat.Core.Modelle;

public sealed class Nachricht
{
    public Nachricht(Guid id, string text, DateTimeOffset zeit, Antwort antwort)
    {
        Id = id;
        Text = text;
        Zeit = zeit;
        Antwort = antwort;
    }

    public Guid Id { get; }

    public string Text { get; }

    public DateTimeOffset Zeit { get; }

    public Antwort Antwort { get; }
}
