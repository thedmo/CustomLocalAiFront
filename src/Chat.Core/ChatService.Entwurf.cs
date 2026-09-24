using Chat.Core.Modelle;

namespace Chat.Core;

public sealed partial class ChatService
{
    public Task<Guid> NeueUnterhaltungAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        Guid id;
        do
        {
            id = Guid.NewGuid();
        }
        while (!_zugriff.Entwuerfe.TryAdd(id, 0));

        return Task.FromResult(id);
    }

    private async Task<AntwortLauf> StarteAntwortAsync(
        Guid unterhaltungId,
        string text,
        CancellationToken ct)
    {
        bool istEntwurf = _zugriff.Entwuerfe.ContainsKey(unterhaltungId);
        Unterhaltung unterhaltung = istEntwurf
            ? new Unterhaltung(unterhaltungId, "Neue Unterhaltung", DateTimeOffset.UtcNow)
            : await _store.LadenAsync(unterhaltungId, ct);
        Antwort antwort = new(Guid.NewGuid());
        Nachricht nachricht = new(Guid.NewGuid(), text, DateTimeOffset.UtcNow, antwort);
        unterhaltung.FuegeNachrichtHinzu(nachricht);
        await _store.SpeichernAsync(unterhaltung, ct);
        if (istEntwurf)
        {
            _zugriff.Entwuerfe.TryRemove(unterhaltungId, out _);
        }

        AktiveAnfrage anfrage = new(unterhaltung, antwort, _konfiguration.ZeitlimitSekunden, ct);
        if (!_zugriff.Anfragen.TryAdd(antwort.Id, anfrage))
        {
            anfrage.Dispose();
            throw new InvalidOperationException("Die Antwort-ID ist bereits aktiv.");
        }

        _ = BeobachteLebenszyklusAsync(anfrage);
        return new AntwortLauf(antwort.Id, StreameAntwortAsync(anfrage));
    }
}
