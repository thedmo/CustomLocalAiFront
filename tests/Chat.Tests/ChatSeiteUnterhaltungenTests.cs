using AngleSharp.Dom;
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
    public async Task Loeschen_LetzteUnterhaltung_LaesstListeLeerUndLegtKeinenErsatzAn()
    {
        FakeChatService service = new();
        Unterhaltung gespeichert = Gespeichert();
        service.Unterhaltungen.Add(gespeichert.Id, gespeichert);
        await using BunitContext context = Kontext(service);
        context.JSInterop.Setup<bool>("confirm", _ => true).SetResult(true);
        IRenderedComponent<Home> seite = context.Render<Home>();
        await seite.Find($"[data-unterhaltung-id='{gespeichert.Id}']").ClickAsync(new MouseEventArgs());
        await seite.Find("#unterhaltung-loeschen").ClickAsync(new MouseEventArgs());
        Assert.False(service.Unterhaltungen.ContainsKey(gespeichert.Id));
        Assert.Empty(seite.FindAll(".unterhaltungs-eintrag"));
        Assert.Empty(seite.FindAll(".chat-nachricht"));
        Assert.Equal(0, service.NeueUnterhaltungAufrufe);
        Assert.True(seite.Find("#unterhaltung-loeschen").HasAttribute("disabled"));
        Assert.True(seite.Find("#senden").HasAttribute("disabled"));
        Assert.False(seite.Find("#neue-unterhaltung").HasAttribute("disabled"));
    }

    [Fact]
    public async Task Senden_InzwischenGeloeschteUnterhaltung_ZeigtHinweisOhneAbsturz()
    {
        FakeChatService service = new();
        Unterhaltung gespeichert = Gespeichert();
        service.Unterhaltungen.Add(gespeichert.Id, gespeichert);
        await using BunitContext context = Kontext(service);
        IRenderedComponent<Home> seite = context.Render<Home>();
        await seite.Find($"[data-unterhaltung-id='{gespeichert.Id}']").ClickAsync(new MouseEventArgs());
        service.Unterhaltungen.Remove(gespeichert.Id);
        seite.Find("#nachricht").Input("Test");
        await seite.Find("#senden").ClickAsync(new MouseEventArgs());
        Assert.Contains("inzwischen gelöscht", seite.Find("[role='alert']").TextContent);
        Assert.False(seite.Find("#senden").HasAttribute("disabled"));
        Assert.Empty(seite.FindAll(".chat-nachricht"));
        Assert.False(seite.Find("#neue-unterhaltung").HasAttribute("disabled"));
        await seite.Find("#neue-unterhaltung").ClickAsync(new MouseEventArgs());
        Assert.Empty(seite.FindAll("[role='alert']"));
    }

    [Fact]
    public async Task Loeschen_AktualisierungScheitert_ZeigtKeinenGeloeschtenVerlaufUndBleibtBedienbar()
    {
        FakeChatService service = new();
        Unterhaltung gespeichert = Gespeichert();
        service.Unterhaltungen.Add(gespeichert.Id, gespeichert);
        await using BunitContext context = Kontext(service);
        context.JSInterop.Setup<bool>("confirm", _ => true).SetResult(true);
        IRenderedComponent<Home> seite = context.Render<Home>();
        await seite.Find($"[data-unterhaltung-id='{gespeichert.Id}']").ClickAsync(new MouseEventArgs());
        service.ListenAusnahme = new IOException("Interne Details");
        await seite.Find("#unterhaltung-loeschen").ClickAsync(new MouseEventArgs());
        Assert.False(service.Unterhaltungen.ContainsKey(gespeichert.Id));
        Assert.Empty(seite.FindAll(".unterhaltungs-eintrag"));
        Assert.Contains("verbleibende Liste konnte nicht geladen", seite.Find("[role='alert']").TextContent);
        Assert.True(seite.Find("#senden").HasAttribute("disabled"));
        service.ListenAusnahme = null;
        await seite.Find("#neue-unterhaltung").ClickAsync(new MouseEventArgs());
        Assert.Empty(seite.FindAll("[role='alert']"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Loeschen_Bestaetigung_EntferntNurNachZustimmung(bool bestaetigt)
    {
        FakeChatService service = new();
        Unterhaltung alt = Gespeichert();
        service.Unterhaltungen.Add(alt.Id, alt);
        Unterhaltung behalten = new(Guid.NewGuid(), "Behalten", DateTimeOffset.UtcNow);
        service.Unterhaltungen.Add(behalten.Id, behalten);
        await using BunitContext context = Kontext(service);
        context.JSInterop.Setup<bool>("confirm", _ => true).SetResult(bestaetigt);
        IRenderedComponent<Home> seite = context.Render<Home>();
        await seite.Find($"[data-unterhaltung-id='{alt.Id}']").ClickAsync(new MouseEventArgs());
        await seite.Find("#unterhaltung-loeschen").ClickAsync(new MouseEventArgs());
        Assert.Contains(alt.Titel, (string)context.JSInterop.Invocations["confirm"].Single().Arguments[0]!);
        Assert.True(service.Unterhaltungen.ContainsKey(behalten.Id));
        Assert.Equal(!bestaetigt, service.Unterhaltungen.ContainsKey(alt.Id));
        Assert.Equal(bestaetigt ? alt.Id : (Guid?)null, service.ZuletztGeloescht);
        Assert.Equal(0, service.NeueUnterhaltungAufrufe);
        Assert.Equal(bestaetigt ? 1 : 2, seite.FindAll(".unterhaltungs-eintrag").Count);
        if (bestaetigt)
        {
            Assert.Empty(seite.FindAll(".chat-nachricht"));
            Assert.Equal(behalten.Id.ToString(), seite.Find("[aria-pressed='true']").GetAttribute("data-unterhaltung-id"));
            Assert.Contains("gelöscht", seite.Find("#chat-status").TextContent);
        }
        else
        {
            Assert.Contains("Gespeicherter Teil", seite.Find("#chat-verlauf").TextContent);
            IElement ausgewaehlt = seite.Find("[aria-current='page']");
            Assert.Equal(alt.Id.ToString(), ausgewaehlt.GetAttribute("data-unterhaltung-id"));
            Assert.Contains("unterhaltungs-eintrag--aktiv", ausgewaehlt.ClassList);
        }
    }

    [Fact]
    public async Task Loeschen_Wartet_SperrtWeitereAktionen()
    {
        FakeChatService service = new();
        Unterhaltung gespeichert = Gespeichert();
        service.Unterhaltungen.Add(gespeichert.Id, gespeichert);
        TaskCompletionSource freigabe = new(TaskCreationOptions.RunContinuationsAsynchronously);
        service.LoeschVerzoegerung = freigabe.Task;
        await using BunitContext context = Kontext(service);
        context.JSInterop.Setup<bool>("confirm", _ => true).SetResult(true);
        IRenderedComponent<Home> seite = context.Render<Home>();
        await seite.Find($"[data-unterhaltung-id='{gespeichert.Id}']").ClickAsync(new MouseEventArgs());
        seite.Find("#nachricht").Input("Test");
        Task loeschen = seite.Find("#unterhaltung-loeschen").ClickAsync(new MouseEventArgs());
        seite.WaitForAssertion(() =>
        {
            foreach (string selector in new[] { "#unterhaltung-loeschen", "#neue-unterhaltung", "#senden", "#nachricht", ".unterhaltungs-eintrag" })
            {
                Assert.True(seite.Find(selector).HasAttribute("disabled"));
            }
            Assert.Contains("unterhaltungs-eintrag--aktiv", seite.Find("[aria-current='page']").ClassList);
        });
        freigabe.SetResult();
        await loeschen;
        Assert.True(seite.Find("#unterhaltung-loeschen").HasAttribute("disabled"));
        Assert.False(seite.Find("#neue-unterhaltung").HasAttribute("disabled"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Loeschen_Fehler_ErhaeltVerlaufUndErlaubtErneutenVersuch(bool aktiv)
    {
        FakeChatService service = new();
        service.LoeschAusnahme = aktiv ? new InvalidOperationException("Interne Details") : new IOException("Interne Details");
        Unterhaltung alt = Gespeichert();
        service.Unterhaltungen.Add(alt.Id, alt);
        await using BunitContext context = Kontext(service);
        context.JSInterop.Setup<bool>("confirm", _ => true).SetResult(true);
        IRenderedComponent<Home> seite = context.Render<Home>();
        await seite.Find($"[data-unterhaltung-id='{alt.Id}']").ClickAsync(new MouseEventArgs());
        await seite.Find("#unterhaltung-loeschen").ClickAsync(new MouseEventArgs());
        Assert.Contains("nicht gelöscht", seite.Find("[role='alert']").TextContent);
        Assert.DoesNotContain("Interne Details", seite.Markup);
        Assert.Equal("Störung", seite.Find("#chat-status").TextContent);
        Assert.Contains("Gespeicherter Teil", seite.Find("#chat-verlauf").TextContent);
        Assert.True(service.Unterhaltungen.ContainsKey(alt.Id));
        Assert.False(seite.Find("#unterhaltung-loeschen").HasAttribute("disabled"));
        Assert.Equal(alt.Id.ToString(), seite.Find("[aria-current='page']").GetAttribute("data-unterhaltung-id"));
        service.LoeschAusnahme = null;
        await seite.Find("#unterhaltung-loeschen").ClickAsync(new MouseEventArgs());
        Assert.False(service.Unterhaltungen.ContainsKey(alt.Id));
        Assert.Empty(seite.FindAll("[role='alert']"));
    }

    [Fact]
    public async Task Senden_LoeschenBleibtBisZumAbschlussGesperrt()
    {
        FakeChatService service = new();
        await using BunitContext context = Kontext(service);
        IRenderedComponent<Home> seite = context.Render<Home>();
        seite.Find("#nachricht").Input("Test");
        Task senden = seite.Find("#senden").ClickAsync(new MouseEventArgs());
        await service.SendenGestartet;
        seite.WaitForAssertion(() => Assert.True(seite.Find("#unterhaltung-loeschen").HasAttribute("disabled")));
        service.BeendeAntwort();
        await senden;
        Assert.False(seite.Find("#unterhaltung-loeschen").HasAttribute("disabled"));
    }

    [Fact]
    public async Task Loeschen_EchterSqliteStore_FuenfVerlaeufeNachNeustart()
    {
        CancellationToken ct = Xunit.TestContext.Current.CancellationToken;
        await using SqliteTestdatenbank datei = await SqliteTestdatenbank.ErzeugeAsync();
        SqliteStore store = new(datei);
        List<Unterhaltung> original = [];
        for (int i = 0; i < 5; i++)
        {
            Unterhaltung u = Gespeichert();
            original.Add(u);
            await store.SpeichernAsync(u, ct);
        }
        FakeModelServerClient client = new();
        await using (BunitContext context = Kontext(new ChatService(client, store, new Konfiguration())))
        {
            context.JSInterop.Setup<bool>("confirm", _ => true).SetResult(true);
            IRenderedComponent<Home> seite = context.Render<Home>();
            seite.WaitForAssertion(() => Assert.Equal(5, seite.FindAll(".unterhaltungs-eintrag").Count));
            await seite.Find($"[data-unterhaltung-id='{original[0].Id}']").ClickAsync(new MouseEventArgs());
            await seite.Find("#unterhaltung-loeschen").ClickAsync(new MouseEventArgs());
            Assert.Empty(seite.FindAll($"[data-unterhaltung-id='{original[0].Id}']"));
            Assert.Equal(4, seite.FindAll(".unterhaltungs-eintrag").Count);
        }
        await new SqliteInitialisierung(datei).InitialisierenAsync(ct);
        SqliteStore neu = new(datei);
        await Assert.ThrowsAsync<KeyNotFoundException>(() => neu.LadenAsync(original[0].Id, ct));
        foreach (Unterhaltung u in original.Skip(1))
        {
            Assert.Equal(u.Nachrichten[0].Antwort.Text, (await neu.LadenAsync(u.Id, ct)).Nachrichten[0].Antwort.Text);
        }

        Assert.Equal(0, client.Aufrufe);
    }

    [Fact]
    public async Task Start_EineNeueLeereUnterhaltungUndGespeicherteListe()
    {
        FakeChatService service = new();
        Unterhaltung alt = Gespeichert();
        service.Unterhaltungen.Add(alt.Id, alt);
        await using BunitContext context = Kontext(service);
        IRenderedComponent<Home> seite = context.Render<Home>();
        Assert.Equal(0, service.NeueUnterhaltungAufrufe);
        Assert.Single(seite.FindAll(".unterhaltungs-eintrag"));
        Assert.Empty(seite.FindAll(".chat-nachricht"));
        Assert.Empty(seite.FindAll("[aria-pressed='true']"));
        Assert.Equal("Gespeicherte Frage", seite.Find(".unterhaltungs-eintrag").ChildNodes[0].TextContent.Trim());
        Assert.Contains("01.01.2026", seite.Find($"[data-unterhaltung-id='{alt.Id}'] time").TextContent);
    }

    [Fact]
    public async Task MobileSeitenleiste_StartetGeschlossenUndLaesstSichUmschalten()
    {
        FakeChatService service = new();
        await using BunitContext context = Kontext(service);
        IRenderedComponent<Home> seite = context.Render<Home>();

        IElement umschalten = seite.Find("#unterhaltungen-umschalten");
        Assert.Equal("false", umschalten.GetAttribute("aria-expanded"));
        Assert.DoesNotContain("unterhaltungs-liste--offen", seite.Find("#unterhaltungen").ClassList);

        await umschalten.ClickAsync(new MouseEventArgs());

        Assert.Equal("true", seite.Find("#unterhaltungen-umschalten").GetAttribute("aria-expanded"));
        Assert.Contains("unterhaltungs-liste--offen", seite.Find("#unterhaltungen").ClassList);

        await seite.Find(".seitenleiste-schliessen").ClickAsync(new MouseEventArgs());

        Assert.Equal("false", seite.Find("#unterhaltungen-umschalten").GetAttribute("aria-expanded"));
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
            Assert.True(seite.Find("#unterhaltung-loeschen").HasAttribute("disabled"));
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
