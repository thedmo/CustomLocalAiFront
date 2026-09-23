using Chat.Core;
using Chat.Core.Modelle;

namespace Chat.Tests;

public sealed partial class ChatServiceTests
{
    [Fact]
    public async Task Abbrechen_VorErstemTeil_SpeichertAbgebrochen()
    {
        FakeModelServerClient client = new();
        (ChatService service, _, Unterhaltung unterhaltung) = ErzeugeService(client);
        AntwortLauf lauf = await service.SendeNachrichtAsync(
            unterhaltung.Id,
            "test",
            CancellationToken.None);

        await service.AbbrechenAsync(lauf.AntwortId, CancellationToken.None);
        IReadOnlyList<string> teile = await SammleAsync(lauf.Teile);

        Assert.Empty(teile);
        Assert.Equal(0, client.Aufrufe);
        Assert.Equal(
            AntwortZustand.Abgebrochen,
            unterhaltung.Nachrichten.Single().Antwort.Zustand);
    }

    [Fact]
    public async Task Abbrechen_WaehrendStream_SpeichertTeilUndAbgebrochen()
    {
        FakeModelServerClient client = new()
        {
            Teile = ["Teil"],
            BlockiereNachErstemTeil = true
        };
        (ChatService service, _, Unterhaltung unterhaltung) = ErzeugeService(client);
        AntwortLauf lauf = await service.SendeNachrichtAsync(
            unterhaltung.Id,
            "test",
            CancellationToken.None);
        await using IAsyncEnumerator<string> enumerator = lauf.Teile.GetAsyncEnumerator(
            TestContext.Current.CancellationToken);

        Assert.True(await enumerator.MoveNextAsync());
        Assert.Equal("Teil", enumerator.Current);
        await service.AbbrechenAsync(lauf.AntwortId, CancellationToken.None);

        Assert.False(await enumerator.MoveNextAsync());
        Antwort antwort = unterhaltung.Nachrichten.Single().Antwort;
        Assert.Equal("Teil", antwort.Text);
        Assert.Equal(AntwortZustand.Abgebrochen, antwort.Zustand);
    }

    [Fact]
    public async Task SendeNachricht_Zeitlimit_ZustandGestoert()
    {
        FakeModelServerClient client = new() { BlockiereSofort = true };
        Konfiguration konfiguration = GueltigeKonfiguration(zeitlimitSekunden: 1);
        (ChatService service, _, Unterhaltung unterhaltung) = ErzeugeService(
            client,
            konfiguration);
        AntwortLauf lauf = await service.SendeNachrichtAsync(
            unterhaltung.Id,
            "test",
            CancellationToken.None);

        StoerfallException ausnahme = await Assert.ThrowsAsync<StoerfallException>(
            () => SammleAsync(lauf.Teile));

        Antwort antwort = unterhaltung.Nachrichten.Single().Antwort;
        Assert.Equal(Stoerfall.Zeitueberschreitung, ausnahme.Fall);
        Assert.Equal(AntwortZustand.Gestoert, antwort.Zustand);
        Assert.Equal(Stoerfall.Zeitueberschreitung, antwort.Fall);
    }

    [Fact]
    public async Task Lebenszyklus_VorErstemTeil_SpeichertGestoert()
    {
        FakeModelServerClient client = new();
        (ChatService service, _, Unterhaltung unterhaltung) = ErzeugeService(client);
        using CancellationTokenSource lebenszyklus = new();
        AntwortLauf lauf = await service.SendeNachrichtAsync(
            unterhaltung.Id,
            "test",
            lebenszyklus.Token);

        lebenszyklus.Cancel();
        Antwort antwort = unterhaltung.Nachrichten.Single().Antwort;
        await WarteBisAsync(() => antwort.Zustand == AntwortZustand.Gestoert);
        IReadOnlyList<string> teile = await SammleAsync(lauf.Teile);

        Assert.Empty(teile);
        Assert.Equal(0, client.Aufrufe);
        Assert.Equal(Stoerfall.ModellserverNichtErreichbar, antwort.Fall);
        Assert.Contains("Chat-Seite", antwort.Grund);
    }

    [Fact]
    public async Task Lebenszyklus_WaehrendStream_BewahrtTeilUndSpeichertGestoert()
    {
        FakeModelServerClient client = new()
        {
            Teile = ["Teil"],
            BlockiereNachErstemTeil = true
        };
        (ChatService service, _, Unterhaltung unterhaltung) = ErzeugeService(client);
        using CancellationTokenSource lebenszyklus = new();
        AntwortLauf lauf = await service.SendeNachrichtAsync(
            unterhaltung.Id,
            "test",
            lebenszyklus.Token);
        await using IAsyncEnumerator<string> enumerator = lauf.Teile.GetAsyncEnumerator(
            TestContext.Current.CancellationToken);

        Assert.True(await enumerator.MoveNextAsync());
        lebenszyklus.Cancel();

        StoerfallException ausnahme = await Assert.ThrowsAsync<StoerfallException>(
            async () => await enumerator.MoveNextAsync());
        Antwort antwort = unterhaltung.Nachrichten.Single().Antwort;
        Assert.Equal(Stoerfall.ModellserverNichtErreichbar, ausnahme.Fall);
        Assert.Equal(AntwortZustand.Gestoert, antwort.Zustand);
        Assert.Equal("Teil", antwort.Text);
    }

    private static async Task WarteBisAsync(Func<bool> bedingung)
    {
        using CancellationTokenSource zeitlimit = new(TimeSpan.FromSeconds(1));
        while (!bedingung())
        {
            await Task.Delay(10, zeitlimit.Token);
        }
    }
}
