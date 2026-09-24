using Chat.Core;
using Chat.Core.Modelle;
using Chat.Store.Sqlite;

namespace Chat.Tests;

public sealed class ChatServicePersistenzTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Stream_TeileUndEndzustandSindDauerhaft(bool abbrechen)
    {
        await using SqliteTestdatenbank datei = await SqliteTestdatenbank.ErzeugeAsync();
        SqliteStore store = new(datei);
        ChatService service = ErzeugeService(store);
        Guid id = await service.NeueUnterhaltungAsync(Ct);
        AntwortLauf lauf = await service.SendeNachrichtAsync(id, "Test", Ct);
        Assert.Equal(AntwortZustand.Angefordert, Assert.Single((await store.LadenAsync(id, Ct)).Nachrichten).Antwort.Zustand);
        await using IAsyncEnumerator<string> stream = lauf.Teile.GetAsyncEnumerator(Ct);
        Assert.True(await stream.MoveNextAsync());
        Antwort gespeichert = Assert.Single((await store.LadenAsync(id, Ct)).Nachrichten).Antwort;
        Assert.Equal("Antwort", gespeichert.Text);
        Assert.Equal(AntwortZustand.Laeuft, gespeichert.Zustand);
        if (abbrechen)
        {
            await service.AbbrechenAsync(lauf.AntwortId, Ct);
        }

        Assert.False(await stream.MoveNextAsync());
        gespeichert = Assert.Single((await store.LadenAsync(id, Ct)).Nachrichten).Antwort;
        Assert.Equal(abbrechen ? AntwortZustand.Abgebrochen : AntwortZustand.Fertig, gespeichert.Zustand);
        Assert.Equal("Antwort", gespeichert.Text);
    }

    [Fact]
    public async Task Abbruch_WaehrendTeilspeicherung_EndzustandWirdNichtUeberschrieben()
    {
        await using SqliteTestdatenbank datei = await SqliteTestdatenbank.ErzeugeAsync();
        VerzoegerterStore store = new(new SqliteStore(datei));
        ChatService service = ErzeugeService(store);
        Guid id = await service.NeueUnterhaltungAsync(Ct);
        AntwortLauf lauf = await service.SendeNachrichtAsync(id, "Test", Ct);
        await using IAsyncEnumerator<string> stream = lauf.Teile.GetAsyncEnumerator(Ct);
        Task<bool> teil = stream.MoveNextAsync().AsTask();
        await store.TeilGestartet.Task.WaitAsync(Ct);
        Task abbruch = service.AbbrechenAsync(lauf.AntwortId, Ct);
        Assert.False(abbruch.IsCompleted);
        store.Freigabe.SetResult();
        Assert.True(await teil);
        await abbruch;
        Assert.False(await stream.MoveNextAsync());
        Antwort a = Assert.Single((await store.LadenAsync(id, Ct)).Nachrichten).Antwort;
        Assert.Equal(AntwortZustand.Abgebrochen, a.Zustand);
        Assert.Equal("Antwort", a.Text);
    }

    private static ChatService ErzeugeService(IStore store) => new(
        new FakeModelServerClient(), store, new Konfiguration
        {
            AdresseModellserver = "http://model-runner.invalid/engines/v1/",
            Modellname = "test",
            ZeitlimitSekunden = 30
        });

    private sealed class VerzoegerterStore(IStore inner) : IStore
    {
        public TaskCompletionSource TeilGestartet { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Freigabe { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public Task<IReadOnlyList<UnterhaltungInfo>> ListeAsync(CancellationToken ct) => inner.ListeAsync(ct);
        public Task<Unterhaltung> LadenAsync(Guid id, CancellationToken ct) => inner.LadenAsync(id, ct);
        public Task LoeschenAsync(Guid id, CancellationToken ct) => inner.LoeschenAsync(id, ct);

        public async Task SpeichernAsync(Unterhaltung u, CancellationToken ct)
        {
            if (u.Nachrichten.LastOrDefault()?.Antwort.Zustand == AntwortZustand.Laeuft)
            {
                TeilGestartet.TrySetResult();
                await Freigabe.Task.WaitAsync(Ct);
            }

            await inner.SpeichernAsync(u, ct);
        }
    }
}
