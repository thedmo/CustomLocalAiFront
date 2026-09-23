using System.Net;
using System.Text;
using Chat.Adapter.ModelRunner;
using Chat.Core;
using Chat.Core.Modelle;

namespace Chat.Tests;

public sealed partial class ChatServiceTests
{
    [Fact(Explicit = true)]
    [Trait("Art", "Integration")]
    public async Task ModelRunnerClient_EchterServer_LiefertTeile()
    {
        string adresse = Environment.GetEnvironmentVariable("MODEL_RUNNER_URL")
            ?? throw new InvalidOperationException("MODEL_RUNNER_URL fehlt.");
        string modell = Environment.GetEnvironmentVariable("MODEL_NAME")
            ?? throw new InvalidOperationException("MODEL_NAME fehlt.");
        Konfiguration konfiguration = GueltigeKonfiguration(adresse, modell);
        using HttpClient http = new();
        ModelRunnerClient client = new(http, konfiguration);
        Antwort antwort = new(Guid.NewGuid());
        Nachricht nachricht = new(Guid.NewGuid(), "test", DateTimeOffset.UtcNow, antwort);

        IReadOnlyList<string> teile = await SammleAsync(client.StreamAntwortAsync(
            [nachricht],
            konfiguration.Systemanweisung,
            TestContext.Current.CancellationToken));

        Assert.NotEmpty(teile);
    }

    [Fact]
    public async Task ModelRunnerClient_StreamMitDone_LiefertTeile()
    {
        const string stream = """
            data: {"choices":[{"delta":{"content":"Teil 1"}}]}

            data: {"choices":[{"delta":{"content":"Teil 2"}}]}

            data: [DONE]

            """;
        using HttpClient http = ErzeugeHttpClient(HttpStatusCode.OK, stream);
        ModelRunnerClient client = new(http, GueltigeKonfiguration());

        IReadOnlyList<string> teile = await SammleAsync(client.StreamAntwortAsync(
            [],
            "Testanweisung",
            TestContext.Current.CancellationToken));

        Assert.Equal(["Teil 1", "Teil 2"], teile);
    }

    [Fact]
    public async Task ModelRunnerClient_StreamOhneDone_WirftStoerfall()
    {
        const string stream = "data: {\"choices\":[{\"delta\":{\"content\":\"Teil\"}}]}\n\n";
        using HttpClient http = ErzeugeHttpClient(HttpStatusCode.OK, stream);
        ModelRunnerClient client = new(http, GueltigeKonfiguration());

        StoerfallException ausnahme = await Assert.ThrowsAsync<StoerfallException>(
            () => SammleAsync(client.StreamAntwortAsync(
                [],
                "Testanweisung",
                TestContext.Current.CancellationToken)));

        Assert.Equal(Stoerfall.ModellserverNichtErreichbar, ausnahme.Fall);
        Assert.Contains("regulären Abschluss", ausnahme.Message);
    }

    [Theory]
    [InlineData("{\"error\":\"model test not found\"}", Stoerfall.ModellUnbekannt)]
    [InlineData("{\"error\":\"route not found\"}", Stoerfall.ModellserverNichtErreichbar)]
    public async Task ModelRunnerClient_NotFound_UnterscheidetModellUndRoute(
        string fehlerinhalt,
        Stoerfall erwarteterFall)
    {
        using HttpClient http = ErzeugeHttpClient(HttpStatusCode.NotFound, fehlerinhalt);
        ModelRunnerClient client = new(http, GueltigeKonfiguration());

        StoerfallException ausnahme = await Assert.ThrowsAsync<StoerfallException>(
            () => SammleAsync(client.StreamAntwortAsync(
                [],
                "Testanweisung",
                TestContext.Current.CancellationToken)));

        Assert.Equal(erwarteterFall, ausnahme.Fall);
    }

    private static HttpClient ErzeugeHttpClient(HttpStatusCode status, string inhalt)
    {
        return new HttpClient(new AntwortHandler(status, inhalt));
    }

    private sealed class AntwortHandler(HttpStatusCode status, string inhalt) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            HttpResponseMessage antwort = new(status)
            {
                Content = new StringContent(inhalt, Encoding.UTF8)
            };
            return Task.FromResult(antwort);
        }
    }
}
