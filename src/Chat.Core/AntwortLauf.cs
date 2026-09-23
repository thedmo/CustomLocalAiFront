namespace Chat.Core;

public sealed record AntwortLauf(Guid AntwortId, IAsyncEnumerable<string> Teile);
