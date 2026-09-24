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
    return new Konfiguration
    {
        AdresseModellserver = configuration["MODEL_RUNNER_URL"]
            ?? configuration["Chat:AdresseModellserver"]
            ?? string.Empty,
        Modellname = configuration["MODEL_NAME"]
            ?? configuration["Chat:Modellname"]
            ?? string.Empty,
        Systemanweisung = configuration["Chat:Systemanweisung"]
            ?? standard.Systemanweisung,
        Temperatur = configuration.GetValue("Chat:Temperatur", standard.Temperatur),
        MaximaleAntwortlaenge = configuration.GetValue(
            "Chat:MaximaleAntwortlaenge",
            standard.MaximaleAntwortlaenge),
        Eingabegrenze = configuration.GetValue(
            "Chat:Eingabegrenze",
            standard.Eingabegrenze),
        ZeitlimitSekunden = configuration.GetValue(
            "Chat:ZeitlimitSekunden",
            standard.ZeitlimitSekunden),
        SpeicherortDb = configuration["Chat:SpeicherortDb"] ?? string.Empty,
        Protokolldatei = configuration["Chat:Protokolldatei"] ?? string.Empty
    };
}
