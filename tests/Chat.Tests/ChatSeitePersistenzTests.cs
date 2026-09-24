using Bunit;
using Chat.Core;
using Chat.Core.Modelle;
using Chat.Store.Sqlite;
using LocalAiFront.Components.Pages;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;

namespace Chat.Tests;

public sealed class ChatSeitePersistenzTests
{
    [Fact]
    public async Task Start_LeereSqliteDatenbank_BleibtBisZurErstenNachrichtLeer()
    {
        CancellationToken ct = Xunit.TestContext.Current.CancellationToken;
        await using SqliteTestdatenbank datei = await SqliteTestdatenbank.ErzeugeAsync();
        SqliteStore store = new(datei);
        FakeModelServerClient client = new();
        ChatService service = new(client, store, new Konfiguration
        {
            AdresseModellserver = "http://model-runner.invalid/engines/v1/",
            Modellname = "test"
        });
        await using BunitContext context = Kontext(service);
        IRenderedComponent<Home> seite = context.Render<Home>();
        Assert.Empty(seite.FindAll(".unterhaltungs-eintrag"));
        Assert.Empty(await store.ListeAsync(ct));
        Assert.False(seite.Find("#nachricht").HasAttribute("disabled"));
        Assert.True(seite.Find("#senden").HasAttribute("disabled"));
        Assert.Equal(0, client.Aufrufe);
        seite.Find("#nachricht").Input("Erste Frage");
        await seite.Find("#senden").ClickAsync(new MouseEventArgs());
        UnterhaltungInfo info = Assert.Single(await store.ListeAsync(ct));
        Unterhaltung gespeichert = await store.LadenAsync(info.Id, ct);
        Assert.Equal("Erste Frage", Assert.Single(gespeichert.Nachrichten).Text);
        Assert.Equal(info.Id.ToString(), seite.Find("[aria-pressed='true']").GetAttribute("data-unterhaltung-id"));
        Assert.Equal(1, client.Aufrufe);
    }

    [Fact]
    public async Task NeueSeite_NachNeustart_LiestEchtenSqliteVerlaufOhneModell()
    {
        CancellationToken ct = Xunit.TestContext.Current.CancellationToken;
        await using SqliteTestdatenbank datei = await SqliteTestdatenbank.ErzeugeAsync();
        Unterhaltung alt = Gespeichert();
        await new SqliteStore(datei).SpeichernAsync(alt, ct);
        await new SqliteInitialisierung(datei).InitialisierenAsync(ct);
        FakeModelServerClient client = new();
        ChatService service = new(client, new SqliteStore(datei), new Konfiguration());
        await using BunitContext context = Kontext(service);
        IRenderedComponent<Home> seite = context.Render<Home>();
        seite.WaitForAssertion(() => Assert.Single(seite.FindAll(".unterhaltungs-eintrag")));
        await seite.Find($"[data-unterhaltung-id='{alt.Id}']").ClickAsync(new MouseEventArgs());
        Assert.Contains("Gespeicherter Teil", seite.Find("#chat-verlauf").TextContent);
        Assert.Equal(0, client.Aufrufe);
    }

    private static Unterhaltung Gespeichert()
    {
        Unterhaltung u = new(Guid.NewGuid(), "Alt", new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero));
        u.FuegeNachrichtHinzu(new Nachricht(Guid.NewGuid(), "Gespeicherte Frage", u.ErstelltAm,
            Antwort.Wiederherstellen(Guid.NewGuid(), "Gespeicherter Teil", AntwortZustand.Gestoert, null, null, "Neustart")));
        return u;
    }

    private static BunitContext Kontext(IChatService service)
    {
        BunitContext context = new();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        context.Services.AddSingleton(service);
        return context;
    }
}
