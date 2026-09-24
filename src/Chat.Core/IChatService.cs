using Chat.Core.Modelle;

namespace Chat.Core;

public interface IChatService
{
    /// <summary>Löscht eine Unterhaltung vollständig, sofern keine Antwort aktiv ist.</summary>
    Task LoescheUnterhaltungAsync(Guid id, CancellationToken ct);

    /// <summary>Liefert Kennung, Titel und Datum, neueste Unterhaltung zuerst.</summary>
    Task<IReadOnlyList<UnterhaltungInfo>> ListeUnterhaltungenAsync(CancellationToken ct);

    /// <summary>Lädt den gespeicherten Verlauf unabhängig vom Modellserver.</summary>
    Task<Unterhaltung> OeffneUnterhaltungAsync(Guid id, CancellationToken ct);

    /// <summary>Reserviert die Kennung eines ungespeicherten Entwurfs.</summary>
    Task<Guid> NeueUnterhaltungAsync(CancellationToken ct);

    /// <summary>Sendet eine Nachricht und liefert die Antwort-ID vor dem ersten Antwortteil.</summary>
    Task<AntwortLauf> SendeNachrichtAsync(
        Guid unterhaltungId,
        string text,
        CancellationToken ct);

    /// <summary>Bricht eine aktive Antwort anhand ihrer Kennung ab.</summary>
    Task AbbrechenAsync(Guid antwortId, CancellationToken ct);
}
