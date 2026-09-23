namespace Chat.Core;

public interface IChatService
{
    /// <summary>Erstellt eine neue leere Unterhaltung und liefert ihre Kennung.</summary>
    Task<Guid> NeueUnterhaltungAsync(CancellationToken ct);

    /// <summary>Sendet eine Nachricht und liefert die Antwort-ID vor dem ersten Antwortteil.</summary>
    Task<AntwortLauf> SendeNachrichtAsync(
        Guid unterhaltungId,
        string text,
        CancellationToken ct);

    /// <summary>Bricht eine aktive Antwort anhand ihrer Kennung ab.</summary>
    Task AbbrechenAsync(Guid antwortId, CancellationToken ct);
}
