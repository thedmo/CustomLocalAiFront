using Bunit;
using Chat.Core;
using Chat.Core.Modelle;
using Chat.Store.Sqlite;
using LocalAiFront.Components.Pages;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;

namespace Chat.Tests;

public sealed class ChatSeiteUnterhaltungenTests
{
    [Fact]
    public async Task Start_EineNeueLeereUnterhaltungUndGespeicherteListe()
    {
        FakeChatService service = new();
        Unterhaltung alt = Gespeichert();
        service.Unterhaltungen.Add(alt.Id, alt);
        await using BunitContext context = Kontext(service);
        IRenderedComponent<Home> seite = context.Render<Home>();
        Assert.Equal(1, service.NeueUnterhaltungAufrufe);
        Assert.Equal(2, seite.FindAll(".unterhaltungs-eintrag").Count);
        Assert.Empty(seite.FindAll(".chat-nachricht"));
        Assert.Equal(service.UnterhaltungId.ToString(), seite.Find("[aria-pressed='true']").GetAttribute("data-unterhaltung-id"));
        Assert.Equal("Neue Unterhaltung", seite.Find(".unterhaltungs-eintrag").ChildNodes[0].TextContent.Trim());
        Assert.Contains("01.01.2026", seite.Find($"[data-unterhaltung-id='{alt.Id}'] time").TextContent);
    }

    [Fact]
    public async Task Auswahl_ZeigtGespeicherteAntwortMitNeustartgrundUndSendetAnKennung()
    {
        FakeChatService service = new();
        Unterhaltung alt = Gespeichert();
        service.Unterhaltungen.Add(alt.Id, alt);
        await using BunitContext context = Kontext(service);
        IRenderedComponent<Home> seite = context.Render<Home>();
        await seite.Find($"[data-unterhaltung-id='{alt.Id}']").ClickAsync(new MouseEventArgs());
        Assert.Equal(alt.Id, service.ZuletztGeoeffnet);
        Assert.Contains("Gespeicherter Teil", seite.Find("#chat-verlauf").TextContent);
        Assert.Contains("Neustart", seite.Find("#chat-verlauf").TextContent);
        Assert.Equal(alt.Id.ToString(), seite.Find("[aria-pressed='true']").GetAttribute("data-unterhaltung-id"));
        seite.Find("#nachricht").Input("Weitere Frage");
        service.BeendeAntwort();
        await seite.Find("#senden").ClickAsync(new MouseEventArgs());
        Assert.Equal(alt.Id, service.ZuletztGesendet);
        Assert.Contains("Gespeicherter Teil", seite.Find("#chat-verlauf").TextContent);
    }

    [Fact]
    public async Task Laden_SperrtKonkurrierendeAktionenUndOeffnetLeereUnterhaltung()
    {
        FakeChatService service = new();
        Unterhaltung leer = new(Guid.NewGuid(), "Leer", DateTimeOffset.UtcNow.AddDays(-1));
        service.Unterhaltungen.Add(leer.Id, leer);
        await using BunitContext context = Kontext(service);
        IRenderedComponent<Home> seite = context.Render<Home>();
        TaskCompletionSource freigabe = new(TaskCreationOptions.RunContinuationsAsynchronously);
        service.LadeVerzoegerung = freigabe.Task;
        Task laden = seite.Find($"[data-unterhaltung-id='{leer.Id}']").ClickAsync(new MouseEventArgs());
        seite.WaitForAssertion(() =>
        {
            Assert.True(seite.Find("#neue-unterhaltung").HasAttribute("disabled"));
            Assert.True(seite.Find("#senden").HasAttribute("disabled"));
            Assert.All(seite.FindAll(".unterhaltungs-eintrag"), button => Assert.True(button.HasAttribute("disabled")));
        });
        freigabe.SetResult();
        await laden;
        Assert.Empty(seite.FindAll(".chat-nachricht"));
        Assert.False(seite.Find("#neue-unterhaltung").HasAttribute("disabled"));
    }

    [Fact]
    public async Task Auswahl_UnbekannteKennung_ZeigtHinweisUndBleibtBedienbar()
    {
        FakeChatService service = new();
        Unterhaltung alt = Gespeichert();
        service.Unterhaltungen.Add(alt.Id, alt);
        await using BunitContext context = Kontext(service);
        IRenderedComponent<Home> seite = context.Render<Home>();
        service.Unterhaltungen.Remove(alt.Id);
        await seite.Find($"[data-unterhaltung-id='{alt.Id}']").ClickAsync(new MouseEventArgs());
        Assert.Contains("nicht mehr vorhanden", seite.Find("[role='alert']").TextContent);
        Assert.False(seite.Find("#neue-unterhaltung").HasAttribute("disabled"));
        Assert.Empty(seite.FindAll(".chat-nachricht"));
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
        seite.WaitForAssertion(() => Assert.Equal(2, seite.FindAll(".unterhaltungs-eintrag").Count));
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
