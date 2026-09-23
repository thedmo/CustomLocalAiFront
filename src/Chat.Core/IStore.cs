using Chat.Core.Modelle;

namespace Chat.Core;

public interface IStore
{
    /// <summary>Speichert eine Unterhaltung als fachliche Transaktion.</summary>
    Task SpeichernAsync(Unterhaltung unterhaltung, CancellationToken ct);

    /// <summary>Lädt eine Unterhaltung mit ihrem vollständigen Verlauf.</summary>
    Task<Unterhaltung> LadenAsync(Guid id, CancellationToken ct);
}
