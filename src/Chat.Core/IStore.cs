using Chat.Core.Modelle;

namespace Chat.Core;

public interface IStore
{
    /// <summary>Entfernt eine Unterhaltung samt Verlauf; fehlende Kennungen gelten als gelöscht.</summary>
    Task LoeschenAsync(Guid id, CancellationToken ct);

    /// <summary>Liefert die gespeicherten Unterhaltungen, neueste zuerst.</summary>
    Task<IReadOnlyList<UnterhaltungInfo>> ListeAsync(CancellationToken ct);

    /// <summary>Speichert eine Unterhaltung als fachliche Transaktion.</summary>
    Task SpeichernAsync(Unterhaltung unterhaltung, CancellationToken ct);

    /// <summary>Lädt eine Unterhaltung mit ihrem vollständigen Verlauf.</summary>
    Task<Unterhaltung> LadenAsync(Guid id, CancellationToken ct);
}
