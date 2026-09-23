using Chat.Core.Modelle;

namespace Chat.Core;

public interface IModelServerClient
{
    /// <summary>Liefert die Modellantwort als Folge von Textteilen.</summary>
    IAsyncEnumerable<string> StreamAntwortAsync(
        IReadOnlyList<Nachricht> verlauf,
        string systemanweisung,
        CancellationToken ct);
}
