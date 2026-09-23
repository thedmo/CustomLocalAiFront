using Bunit;
using Chat.Core;
using LocalAiFront.Components.Pages;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;

namespace Chat.Tests;

public sealed class ChatSeiteTests
{
    [Fact]
    public async Task Senden_ZeigtTeileSofortUndSperrtEingabe()
    {
        FakeChatService service = new();
        await using BunitContext context = ErzeugeKontext(service);
        IRenderedComponent<Home> seite = context.Render<Home>();
        seite.Find("#nachricht").Input("Wie geht es?");

        Task senden = seite.Find("#senden").ClickAsync(new MouseEventArgs());
        await service.SendenGestartet;
        await service.LiefereTeilAsync("Teil 1");

        seite.WaitForAssertion(() =>
        {
            Assert.Contains("Wie geht es?", seite.Markup);
            Assert.Contains("Teil 1", seite.Markup);
            Assert.True(seite.Find("#nachricht").HasAttribute("disabled"));
            Assert.NotNull(seite.Find("#abbrechen"));
        });

        await service.LiefereTeilAsync(" Teil 2");
        service.BeendeAntwort();
        await senden;

        Assert.Contains("Teil 1 Teil 2", seite.Markup);
        Assert.False(seite.Find("#nachricht").HasAttribute("disabled"));
        Assert.Empty(seite.FindAll("#abbrechen"));
        Assert.Contains("Bereit", seite.Find("#chat-status").TextContent);
    }

    [Fact]
    public async Task Abbrechen_BewahrtTeilantwortUndGibtEingabeFrei()
    {
        FakeChatService service = new();
        await using BunitContext context = ErzeugeKontext(service);
        IRenderedComponent<Home> seite = context.Render<Home>();
        seite.Find("#nachricht").Input("Lange Antwort");
        Task senden = seite.Find("#senden").ClickAsync(new MouseEventArgs());
        await service.SendenGestartet;
        await service.LiefereTeilAsync("Vorhandener Teil");
        seite.WaitForAssertion(() => Assert.Contains("Vorhandener Teil", seite.Markup));

        await seite.Find("#abbrechen").ClickAsync(new MouseEventArgs());
        await senden;

        Assert.Equal(1, service.AbbruchAufrufe);
        Assert.Contains("Vorhandener Teil", seite.Markup);
        Assert.Contains("Antwort abgebrochen", seite.Markup);
        Assert.False(seite.Find("#nachricht").HasAttribute("disabled"));

        seite.Find("#nachricht").Input("Nächste Frage");
        await seite.Find("#senden").ClickAsync(new MouseEventArgs());

        Assert.Equal(2, service.SendeAufrufe);
        Assert.Contains("Nächste Frage", seite.Markup);
    }

    [Fact]
    public async Task Stoerfall_ZeigtGrundUndGibtEingabeFrei()
    {
        FakeChatService service = new()
        {
            SendeAusnahme = new StoerfallException(
                Stoerfall.ModellserverNichtErreichbar,
                "Modellserver nicht erreichbar")
        };
        await using BunitContext context = ErzeugeKontext(service);
        IRenderedComponent<Home> seite = context.Render<Home>();
        seite.Find("#nachricht").Input("Frage");

        await seite.Find("#senden").ClickAsync(new MouseEventArgs());

        Assert.Contains("Modellserver nicht erreichbar", seite.Find("[role=alert]").TextContent);
        Assert.False(seite.Find("#nachricht").HasAttribute("disabled"));
        Assert.Contains("Störung", seite.Find("#chat-status").TextContent);
    }

    [Fact]
    public async Task NeueUnterhaltung_StartetNeueLeereUnterhaltungUndBehaeltAlte()
    {
        FakeChatService service = new();
        await using BunitContext context = ErzeugeKontext(service);
        IRenderedComponent<Home> seite = context.Render<Home>();

        seite.Find("#nachricht").Input("Erste Frage");
        Task senden = seite.Find("#senden").ClickAsync(new MouseEventArgs());
        await service.SendenGestartet;
        service.BeendeAntwort();
        await senden;

        Assert.Equal(1, service.SendeAufrufe);
        Assert.Single(seite.FindAll(".unterhaltungs-eintrag"));

        await seite.Find("#neue-unterhaltung").ClickAsync(new MouseEventArgs());

        Assert.Equal(2, service.NeueUnterhaltungAufrufe);
        Assert.Equal(2, seite.FindAll(".unterhaltungs-eintrag").Count);
        Assert.Single(seite.FindAll(".unterhaltungs-eintrag[aria-pressed='true']"));
        Assert.Contains("Neue Unterhaltung", seite.Markup);
    }

    [Fact]
    public async Task LeereEingabe_RuftServiceNichtAuf()
    {
        FakeChatService service = new();
        await using BunitContext context = ErzeugeKontext(service);
        IRenderedComponent<Home> seite = context.Render<Home>();

        Assert.True(seite.Find("#senden").HasAttribute("disabled"));
        Assert.Equal(0, service.SendeAufrufe);
    }

    private static BunitContext ErzeugeKontext(FakeChatService service)
    {
        BunitContext context = new();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        context.Services.AddSingleton<IChatService>(service);
        return context;
    }
}
