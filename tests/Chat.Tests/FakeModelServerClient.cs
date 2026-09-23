using System.Runtime.CompilerServices;
using Chat.Core;
using Chat.Core.Modelle;

namespace Chat.Tests;

public sealed class FakeModelServerClient : IModelServerClient
{
    public IReadOnlyList<string> Teile { get; init; } = ["Antwort"];

    public StoerfallException? Ausnahme { get; init; }

    public bool BlockiereNachErstemTeil { get; init; }

    public bool BlockiereSofort { get; init; }

    public int Aufrufe { get; private set; }

    public async IAsyncEnumerable<string> StreamAntwortAsync(
        IReadOnlyList<Nachricht> verlauf,
        string systemanweisung,
        [EnumeratorCancellation] CancellationToken ct)
    {
        Aufrufe++;

        if (Ausnahme is not null)
        {
            throw Ausnahme;
        }

        if (BlockiereSofort)
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, ct);
        }

        for (int index = 0; index < Teile.Count; index++)
        {
            ct.ThrowIfCancellationRequested();
            yield return Teile[index];

            if (BlockiereNachErstemTeil && index == 0)
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, ct);
            }
        }
    }
}
