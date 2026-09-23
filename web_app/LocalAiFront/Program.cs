using LocalAiFront.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Bindet an LiteLLM als OpenAI-kompatiblen Endpunkt; BaseUrl unterscheidet sich je nach Umgebung (siehe appsettings*.json).
builder.Services.AddHttpClient("ModelRunner", client =>
{
    var baseUrl = builder.Configuration["MODEL_RUNNER_URL"]
        ?? builder.Configuration["Chat:AdresseModellserver"]
        ?? throw new InvalidOperationException(
            "Configuration value 'MODEL_RUNNER_URL' is missing.");

    client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
});

var app = builder.Build();

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
