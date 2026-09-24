using System.Globalization;
using Chat.Adapter.ModelRunner;
using Chat.Core;
using Chat.Store.Sqlite;
using LocalAiFront.Components;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

Konfiguration chatKonfiguration = LadeChatKonfiguration(builder.Configuration);
chatKonfiguration.Pruefen();
builder.Services.AddSingleton(chatKonfiguration);
if (string.IsNullOrWhiteSpace(chatKonfiguration.SpeicherortDb))
{
    throw new InvalidOperationException("Chat:SpeicherortDb muss einen SQLite-Dateipfad enthalten.");
}

string datenbankPfad = Path.GetFullPath(chatKonfiguration.SpeicherortDb);
Directory.CreateDirectory(Path.GetDirectoryName(datenbankPfad)!);
var verbindung = new SqliteConnectionStringBuilder { DataSource = datenbankPfad };
builder.Services.AddDbContextFactory<ChatDbContext>(options =>
    options.UseSqlite(verbindung.ToString()).EnableSensitiveDataLogging(false));
builder.Services.AddSingleton<IStore, SqliteStore>();
builder.Services.AddSingleton<SqliteInitialisierung>();
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddHttpClient<IModelServerClient, ModelRunnerClient>();

var app = builder.Build();
await app.Services.GetRequiredService<SqliteInitialisierung>()
    .InitialisierenAsync(CancellationToken.None);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

static Konfiguration LadeChatKonfiguration(IConfiguration configuration)
{
    Konfiguration standard = new();
    string temperaturWert = Konfiguration.NormalisiereText(configuration["TEMPERATURE"], standard.Temperatur.ToString(CultureInfo.InvariantCulture));
    string maxAntwortWert = Konfiguration.NormalisiereText(configuration["MAX_OUTPUT_LENGTH"], standard.MaximaleAntwortlaenge.ToString(CultureInfo.InvariantCulture));
    string maxEingabeWert = Konfiguration.NormalisiereText(configuration["MAX_INPUT_LENGTH"], standard.Eingabegrenze.ToString(CultureInfo.InvariantCulture));
    string zeitlimitWert = Konfiguration.NormalisiereText(configuration["TIMEOUT_SECONDS"], standard.ZeitlimitSekunden.ToString(CultureInfo.InvariantCulture));

    return new Konfiguration
    {
        AdresseModellserver = Konfiguration.NormalisiereText(configuration["MODEL_RUNNER_URL"]),
        Modellname = Konfiguration.NormalisiereText(configuration["MODEL_NAME"]),
        Systemanweisung = Konfiguration.NormalisiereText(configuration["SYSTEM_PROMPT"], standard.Systemanweisung),
        Temperatur = double.TryParse(temperaturWert, NumberStyles.Float, CultureInfo.InvariantCulture, out double temperatur)
            ? temperatur
            : standard.Temperatur,
        MaximaleAntwortlaenge = int.TryParse(maxAntwortWert, NumberStyles.Integer, CultureInfo.InvariantCulture, out int maxAntwort)
            ? maxAntwort
            : standard.MaximaleAntwortlaenge,
        Eingabegrenze = int.TryParse(maxEingabeWert, NumberStyles.Integer, CultureInfo.InvariantCulture, out int maxEingabe)
            ? maxEingabe
            : standard.Eingabegrenze,
        ZeitlimitSekunden = int.TryParse(zeitlimitWert, NumberStyles.Integer, CultureInfo.InvariantCulture, out int zeitlimit)
            ? zeitlimit
            : standard.ZeitlimitSekunden,
        SpeicherortDb = Konfiguration.NormalisiereText(configuration["DB_PATH"]),
        Protokolldatei = Konfiguration.NormalisiereText(configuration["LOG_PATH"])
    };
}
