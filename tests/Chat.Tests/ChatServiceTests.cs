using Chat.Core;
using Chat.Core.Modelle;

namespace Chat.Tests;

public sealed partial class ChatServiceTests
{
    [Fact]
    public async Task SendeNachricht_LeererText_WirftStoerfall()
    {
        FakeModelServerClient client = new();
        ChatService service = new(client, new SpeicherStore(), GueltigeKonfiguration());

        StoerfallException ausnahme = await Assert.ThrowsAsync<StoerfallException>(
            () => service.SendeNachrichtAsync(Guid.NewGuid(), "   ", CancellationToken.None));

        Assert.Equal(Stoerfall.KonfigurationUngueltig, ausnahme.Fall);
        Assert.Equal(0, client.Aufrufe);
    }

    [Fact]
    public async Task SendeNachricht_ZuLang_WirftStoerfall()
    {
        FakeModelServerClient client = new();
        Konfiguration konfiguration = GueltigeKonfiguration(eingabegrenze: 4);
        ChatService service = new(client, new SpeicherStore(), konfiguration);

        StoerfallException ausnahme = await Assert.ThrowsAsync<StoerfallException>(
            () => service.SendeNachrichtAsync(Guid.NewGuid(), "12345", CancellationToken.None));

        Assert.Equal(Stoerfall.KonfigurationUngueltig, ausnahme.Fall);
        Assert.Contains("4", ausnahme.Message);
        Assert.Equal(0, client.Aufrufe);
    }

    [Fact]
    public async Task SendeNachricht_FakeAdapter_LiefertAntwortIdTeileUndFertig()
    {
        FakeModelServerClient client = new() { Teile = ["Teil 1", "Teil 2"] };
        (ChatService service, SpeicherStore store, Unterhaltung unterhaltung) =
            ErzeugeService(client);

        AntwortLauf lauf = await service.SendeNachrichtAsync(
            unterhaltung.Id,
            "test",
            CancellationToken.None);

        Assert.NotEqual(Guid.Empty, lauf.AntwortId);
        Assert.Equal(0, client.Aufrufe);

        Antwort antwort = unterhaltung.Nachrichten.Single().Antwort;
        List<string> teile = [];
        await using IAsyncEnumerator<string> enumerator = lauf.Teile.GetAsyncEnumerator(
            TestContext.Current.CancellationToken);

        Assert.True(await enumerator.MoveNextAsync());
        teile.Add(enumerator.Current);
        Assert.Equal(AntwortZustand.Laeuft, antwort.Zustand);
        Assert.Equal("Teil 1", antwort.Text);

        Assert.True(await enumerator.MoveNextAsync());
        teile.Add(enumerator.Current);
        Assert.False(await enumerator.MoveNextAsync());

        Assert.Equal(["Teil 1", "Teil 2"], teile);
        Assert.Equal(AntwortZustand.Fertig, antwort.Zustand);
        Assert.Equal("Teil 1Teil 2", antwort.Text);
        Assert.NotNull(antwort.Dauer);
        Assert.True(store.Speicheraufrufe >= 2);
    }

    [Fact]
    public async Task SendeNachricht_AdapterWirft_ZustandGestoert()
    {
        FakeModelServerClient client = new()
        {
            Ausnahme = new StoerfallException(
                Stoerfall.ModellserverNichtErreichbar,
                "Teststörung")
        };
        (ChatService service, _, Unterhaltung unterhaltung) = ErzeugeService(client);
        AntwortLauf lauf = await service.SendeNachrichtAsync(
            unterhaltung.Id,
            "test",
            CancellationToken.None);

        StoerfallException ausnahme = await Assert.ThrowsAsync<StoerfallException>(
            () => SammleAsync(lauf.Teile));

        Antwort antwort = unterhaltung.Nachrichten.Single().Antwort;
        Assert.Equal(Stoerfall.ModellserverNichtErreichbar, ausnahme.Fall);
        Assert.Equal(AntwortZustand.Gestoert, antwort.Zustand);
        Assert.Equal(Stoerfall.ModellserverNichtErreichbar, antwort.Fall);
    }

    [Fact]
    public void Konfiguration_AdresseLeer_WirftKonfigurationUngueltig()
    {
        Konfiguration konfiguration = new()
        {
            AdresseModellserver = string.Empty,
            Modellname = "lokales-testmodell",
            Systemanweisung = "Testanweisung",
            Temperatur = 0.2,
            MaximaleAntwortlaenge = 128,
            Eingabegrenze = 4000,
            ZeitlimitSekunden = 5
        };

        StoerfallException ausnahme = Assert.Throws<StoerfallException>(konfiguration.Pruefen);

        Assert.Equal(Stoerfall.KonfigurationUngueltig, ausnahme.Fall);
        Assert.Contains(nameof(Konfiguration.AdresseModellserver), ausnahme.Message);
    }

    [Fact]
    public void Konfiguration_ModellnameLeer_WirftKonfigurationUngueltig()
    {
        Konfiguration konfiguration = GueltigeKonfiguration(
            "http://model-runner.invalid/engines/v1/",
            string.Empty);

        StoerfallException ausnahme = Assert.Throws<StoerfallException>(konfiguration.Pruefen);

        Assert.Equal(Stoerfall.KonfigurationUngueltig, ausnahme.Fall);
        Assert.Contains(nameof(Konfiguration.Modellname), ausnahme.Message);
    }

    [Fact]
    public void AntwortZustand_NurErlaubteUebergaenge()
    {
        (AntwortZustand Von, AntwortZustand Nach)[] erlaubteUebergaenge =
        [
            (AntwortZustand.Angefordert, AntwortZustand.Laeuft),
            (AntwortZustand.Angefordert, AntwortZustand.Abgebrochen),
            (AntwortZustand.Angefordert, AntwortZustand.Gestoert),
            (AntwortZustand.Laeuft, AntwortZustand.Fertig),
            (AntwortZustand.Laeuft, AntwortZustand.Abgebrochen),
            (AntwortZustand.Laeuft, AntwortZustand.Gestoert)
        ];

        foreach ((AntwortZustand von, AntwortZustand nach) in erlaubteUebergaenge)
        {
            Antwort antwort = ErzeugeAntwortImZustand(von);
            antwort.WechsleZu(nach);
            Assert.Equal(nach, antwort.Zustand);
        }

        foreach (AntwortZustand von in Enum.GetValues<AntwortZustand>())
        {
            foreach (AntwortZustand nach in Enum.GetValues<AntwortZustand>())
            {
                if (erlaubteUebergaenge.Contains((von, nach)))
                {
                    continue;
                }

                Antwort antwort = ErzeugeAntwortImZustand(von);
                Assert.Throws<InvalidOperationException>(() => antwort.WechsleZu(nach));
            }
        }
    }

    private static Antwort ErzeugeAntwortImZustand(AntwortZustand zustand)
    {
        Antwort antwort = new(Guid.NewGuid());

        if (zustand is AntwortZustand.Laeuft or AntwortZustand.Fertig)
        {
            antwort.WechsleZu(AntwortZustand.Laeuft);
        }

        if (zustand == AntwortZustand.Fertig)
        {
            antwort.WechsleZu(AntwortZustand.Fertig);
        }
        else if (zustand is AntwortZustand.Abgebrochen or AntwortZustand.Gestoert)
        {
            antwort.WechsleZu(zustand);
        }

        return antwort;
    }

    private static (ChatService Service, SpeicherStore Store, Unterhaltung Unterhaltung)
        ErzeugeService(
            FakeModelServerClient client,
            Konfiguration? konfiguration = null)
    {
        SpeicherStore store = new();
        Unterhaltung unterhaltung = new(Guid.NewGuid(), "Neue Unterhaltung", DateTimeOffset.UtcNow);
        store.FuegeHinzu(unterhaltung);
        ChatService service = new(client, store, konfiguration ?? GueltigeKonfiguration());
        return (service, store, unterhaltung);
    }

    private static async Task<IReadOnlyList<string>> SammleAsync(
        IAsyncEnumerable<string> teile)
    {
        List<string> ergebnis = [];
        await foreach (string teil in teile)
        {
            ergebnis.Add(teil);
        }

        return ergebnis;
    }

    private static Konfiguration GueltigeKonfiguration(
        int eingabegrenze = 4000,
        int zeitlimitSekunden = 5)
    {
        return GueltigeKonfiguration(
            "http://model-runner.invalid/engines/v1/",
            "lokales-testmodell",
            eingabegrenze,
            zeitlimitSekunden);
    }

    private static Konfiguration GueltigeKonfiguration(
        string adresse,
        string modell,
        int eingabegrenze = 4000,
        int zeitlimitSekunden = 5)
    {
        return new Konfiguration
        {
            AdresseModellserver = adresse,
            Modellname = modell,
            Systemanweisung = "Testanweisung",
            Temperatur = 0.2,
            MaximaleAntwortlaenge = 128,
            Eingabegrenze = eingabegrenze,
            ZeitlimitSekunden = zeitlimitSekunden
        };
    }

}
