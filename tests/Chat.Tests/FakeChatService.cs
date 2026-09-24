using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Chat.Core;
using Chat.Core.Modelle;

namespace Chat.Tests;

public sealed class FakeChatService : IChatService
{
    public Dictionary<Guid, Unterhaltung> Unterhaltungen { get; } = [];
    public Task? LadeVerzoegerung { get; set; }
    public Guid? ZuletztGeoeffnet { get; private set; }
    public Guid? ZuletztGesendet { get; private set; }
    public Guid? ZuletztGeloescht { get; private set; }
    public Task? LoeschVerzoegerung { get; set; }
    public Exception? LoeschAusnahme { get; set; }
    public Exception? ListenAusnahme { get; set; }

    public async Task LoescheUnterhaltungAsync(Guid id, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ZuletztGeloescht = id;
        if (LoeschVerzoegerung is not null)
        {
            await LoeschVerzoegerung.WaitAsync(ct);
        }
        if (LoeschAusnahme is not null)
        {
            throw LoeschAusnahme;
        }
        Unterhaltungen.Remove(id);
    }

    public Task<IReadOnlyList<UnterhaltungInfo>> ListeUnterhaltungenAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        if (ListenAusnahme is not null)
        {
            throw ListenAusnahme;
        }
        IReadOnlyList<UnterhaltungInfo> liste = Unterhaltungen.Values
            .OrderByDescending(u => u.ErstelltAm).ThenBy(u => u.Id)
            .Select(u => new UnterhaltungInfo(u.Id, u.Titel, u.ErstelltAm)).ToArray();
        return Task.FromResult(liste);
    }

    public async Task<Unterhaltung> OeffneUnterhaltungAsync(Guid id, CancellationToken ct)
    {
        ZuletztGeoeffnet = id;
        if (LadeVerzoegerung is not null)
        {
            await LadeVerzoegerung.WaitAsync(ct);
        }

        return Unterhaltungen[id];
    }

    private readonly Channel<string> _teile = Channel.CreateUnbounded<string>();
    private readonly TaskCompletionSource _sendenGestartet = new(
        TaskCreationOptions.RunContinuationsAsynchronously);

    public Guid UnterhaltungId { get; private set; } = Guid.NewGuid();

    public Guid AntwortId { get; } = Guid.NewGuid();

    public StoerfallException? SendeAusnahme { get; init; }

    public int SendeAufrufe { get; private set; }

    public int NeueUnterhaltungAufrufe { get; private set; }

    public int AbbruchAufrufe { get; private set; }

    public Task SendenGestartet => _sendenGestartet.Task;

    public Task<Guid> NeueUnterhaltungAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        NeueUnterhaltungAufrufe++;
        UnterhaltungId = Guid.NewGuid();
        Unterhaltungen.Add(UnterhaltungId, new Unterhaltung(UnterhaltungId, "Neue Unterhaltung", DateTimeOffset.UtcNow));
        return Task.FromResult(UnterhaltungId);
    }

    public Task<AntwortLauf> SendeNachrichtAsync(
        Guid unterhaltungId,
        string text,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        SendeAufrufe++;
        ZuletztGesendet = unterhaltungId;
        _sendenGestartet.TrySetResult();

        if (SendeAusnahme is not null)
        {
            throw SendeAusnahme;
        }

        Antwort antwort = new(Guid.NewGuid());
        Unterhaltungen[unterhaltungId].FuegeNachrichtHinzu(new Nachricht(Guid.NewGuid(), text, DateTimeOffset.UtcNow, antwort));
        return Task.FromResult(new AntwortLauf(AntwortId, LiesTeileAsync(ct)));
    }

    public Task AbbrechenAsync(Guid antwortId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        AbbruchAufrufe++;
        _teile.Writer.TryComplete();
        return Task.CompletedTask;
    }

    public ValueTask LiefereTeilAsync(string teil)
    {
        return _teile.Writer.WriteAsync(teil);
    }

    public void BeendeAntwort()
    {
        _teile.Writer.TryComplete();
    }

    private async IAsyncEnumerable<string> LiesTeileAsync(
        [EnumeratorCancellation] CancellationToken ct)
    {
        await foreach (string teil in _teile.Reader.ReadAllAsync(ct))
        {
            yield return teil;
        }
    }
}
