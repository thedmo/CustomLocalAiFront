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
            Assert.Contains("chat-statuspunkt--nicht-bereit", seite.Find(".chat-statuspunkt").ClassList);
        });

        await service.LiefereTeilAsync(" Teil 2");
        service.BeendeAntwort();
        await senden;

        Assert.Contains("Teil 1 Teil 2", seite.Markup);
        Assert.False(seite.Find("#nachricht").HasAttribute("disabled"));
        Assert.Empty(seite.FindAll("#abbrechen"));
        Assert.Contains("Bereit", seite.Find("#chat-status").TextContent);
        Assert.Contains("chat-statuspunkt--bereit", seite.Find(".chat-statuspunkt").ClassList);
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

    [Theory]
    [InlineData(Stoerfall.ModellserverNichtErreichbar, "Modellserver nicht erreichbar")]
    [InlineData(Stoerfall.ModellUnbekannt, "Modell unbekannt")]
    [InlineData(Stoerfall.KonfigurationUngueltig, "Konfiguration ungültig")]
    [InlineData(Stoerfall.Zeitueberschreitung, "Zeitüberschreitung")]
    public async Task Stoerfall_ZeigtGrundUndGibtEingabeFrei(Stoerfall fall, string grund)
    {
        FakeChatService service = new()
        {
            SendeAusnahme = new StoerfallException(fall, grund)
        };
        await using BunitContext context = ErzeugeKontext(service);
        IRenderedComponent<Home> seite = context.Render<Home>();
        seite.Find("#nachricht").Input("Frage");

        await seite.Find("#senden").ClickAsync(new MouseEventArgs());

        Assert.Contains(grund, seite.Find("[role=alert]").TextContent);
        Assert.False(seite.Find("#nachricht").HasAttribute("disabled"));
        Assert.False(seite.Find("#senden").HasAttribute("disabled"));
        Assert.Contains("Störung", seite.Find("#chat-status").TextContent);
        Assert.Contains("chat-statuspunkt--nicht-bereit", seite.Find(".chat-statuspunkt").ClassList);
        Assert.Empty(service.Unterhaltungen);
    }

    [Fact]
    public async Task NeueUnterhaltung_StartetNeueLeereUnterhaltungUndBehaeltAlte()
    {
        FakeChatService service = new();
        await using BunitContext context = ErzeugeKontext(service);
        IRenderedComponent<Home> seite = context.Render<Home>();

        Assert.Equal(0, service.NeueUnterhaltungAufrufe);
        Assert.Empty(seite.FindAll(".unterhaltungs-eintrag"));

        seite.Find("#nachricht").Input("Erste Frage");
        Task senden = seite.Find("#senden").ClickAsync(new MouseEventArgs());
        await service.SendenGestartet;
        service.BeendeAntwort();
        await senden;

        Assert.Equal(1, service.SendeAufrufe);
        Assert.Equal(1, service.NeueUnterhaltungAufrufe);
        Assert.Single(seite.FindAll(".unterhaltungs-eintrag"));

        await seite.Find("#neue-unterhaltung").ClickAsync(new MouseEventArgs());

        Assert.Equal(1, service.NeueUnterhaltungAufrufe);
        Assert.Single(seite.FindAll(".unterhaltungs-eintrag"));
        Assert.Empty(seite.FindAll(".unterhaltungs-eintrag[aria-pressed='true']"));
        Assert.Empty(seite.FindAll(".chat-nachricht"));
        Assert.False(seite.Find("#nachricht").HasAttribute("disabled"));
    }

    [Fact]
    public async Task LeereEingabe_ZeigtZugeordnetenHinweisUndRuftServiceNichtAuf()
    {
        FakeChatService service = new();
        await using BunitContext context = ErzeugeKontext(service);
        IRenderedComponent<Home> seite = context.Render<Home>();

        AngleSharp.Dom.IElement senden = seite.Find("#senden");
        AngleSharp.Dom.IElement sendenBereich = seite.Find(".senden-bereich");
        Assert.True(senden.HasAttribute("disabled"));
        Assert.Equal("Nachricht senden", senden.GetAttribute("aria-label"));
        Assert.Empty(senden.TextContent.Trim());
        Assert.Equal("0", sendenBereich.GetAttribute("tabindex"));
        Assert.Equal("senden-hinweis", sendenBereich.GetAttribute("aria-describedby"));
        Assert.Equal("Bitte erst Nachricht eingeben.", seite.Find("#senden-hinweis").TextContent);
        Assert.Contains("chat-statuspunkt--bereit", seite.Find(".chat-statuspunkt").ClassList);
        Assert.Equal(0, service.SendeAufrufe);
    }

    [Fact]
    public async Task Eingabe_ZaehltZeichenUndBegrenztBei4000Zeichen()
    {
        FakeChatService service = new();
        await using BunitContext context = ErzeugeKontext(service);
        IRenderedComponent<Home> seite = context.Render<Home>();
        AngleSharp.Dom.IElement eingabe = seite.Find("#nachricht");

        Assert.Equal("4000", eingabe.GetAttribute("maxlength"));
        Assert.Contains("zeichenzaehler", eingabe.GetAttribute("aria-describedby"));
        Assert.Equal("0/4000", seite.Find("#zeichenzaehler").TextContent.Trim());

        eingabe.Input("A");
        Assert.Equal("1/4000", seite.Find("#zeichenzaehler").TextContent.Trim());

        eingabe.Input(new string('A', 4000));
        Assert.Equal("4000/4000", seite.Find("#zeichenzaehler").TextContent.Trim());
        Assert.False(seite.Find("#senden").HasAttribute("disabled"));
    }

    [Fact]
    public async Task ZuLangeEingabe_ZeigtHinweisUndRuftServiceNichtAuf()
    {
        FakeChatService service = new();
        await using BunitContext context = ErzeugeKontext(service);
        IRenderedComponent<Home> seite = context.Render<Home>();

        seite.Find("#nachricht").Input(new string('A', 4001));

        Assert.True(seite.Find("#senden").HasAttribute("disabled"));
        Assert.Contains("4000 Zeichen", seite.Find("#senden-hinweis").TextContent);
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
