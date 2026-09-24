using System.Collections.Concurrent;
using Chat.Core;
using Chat.Core.Modelle;

namespace Chat.Tests;

public sealed class SpeicherStore : IStore
{
    private readonly ConcurrentDictionary<Guid, Unterhaltung> _unterhaltungen = new();

    public Task LoeschenAsync(Guid id, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        _unterhaltungen.TryRemove(id, out _);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<UnterhaltungInfo>> ListeAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        IReadOnlyList<UnterhaltungInfo> liste = _unterhaltungen.Values
            .OrderByDescending(u => u.ErstelltAm).ThenBy(u => u.Id)
            .Select(u => new UnterhaltungInfo(u.Id, u.Titel, u.ErstelltAm)).ToArray();
        return Task.FromResult(liste);
    }

    public int Speicheraufrufe { get; private set; }

    public void FuegeHinzu(Unterhaltung unterhaltung)
    {
        _unterhaltungen[unterhaltung.Id] = unterhaltung;
    }

    public Task SpeichernAsync(Unterhaltung unterhaltung, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        _unterhaltungen[unterhaltung.Id] = unterhaltung;
        Speicheraufrufe++;
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
