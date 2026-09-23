using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Chat.Core;

namespace Chat.Tests;

public sealed class FakeChatService : IChatService
{
    private readonly Channel<string> _teile = Channel.CreateUnbounded<string>();
    private readonly TaskCompletionSource _sendenGestartet = new(
        TaskCreationOptions.RunContinuationsAsynchronously);

    public Guid UnterhaltungId { get; } = Guid.NewGuid();

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
        return Task.FromResult(UnterhaltungId);
    }

    public Task<AntwortLauf> SendeNachrichtAsync(
        Guid unterhaltungId,
        string text,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        SendeAufrufe++;
        _sendenGestartet.TrySetResult();

        if (SendeAusnahme is not null)
        {
            throw SendeAusnahme;
        }

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
