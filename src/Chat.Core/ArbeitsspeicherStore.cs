using System.Collections.Concurrent;
using Chat.Core.Modelle;

namespace Chat.Core;

public sealed class ArbeitsspeicherStore : IStore
{
    private readonly ConcurrentDictionary<Guid, Unterhaltung> _unterhaltungen = new();

    public Task SpeichernAsync(Unterhaltung unterhaltung, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(unterhaltung);
        _unterhaltungen[unterhaltung.Id] = unterhaltung;
        return Task.CompletedTask;
    }

    public Task<Unterhaltung> LadenAsync(Guid id, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (!_unterhaltungen.TryGetValue(id, out Unterhaltung? unterhaltung))
        {
            throw new KeyNotFoundException($"Unterhaltung {id} wurde nicht gefunden.");
        }

        return Task.FromResult(unterhaltung);
    }
}
