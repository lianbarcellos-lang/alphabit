using Alphabit.App.Components;
using Alphabit.App.Services;

var builder = WebApplication.CreateBuilder(args);
var railwayPort = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(railwayPort))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{railwayPort}");
}

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddHttpClient<AlphabitApiClient>(client =>
{
    var configuredBaseUrl = builder.Configuration["AlphabitApi:BaseUrl"];
    if (string.IsNullOrWhiteSpace(configuredBaseUrl))
    {
        throw new InvalidOperationException(
            "A configuracao 'AlphabitApi:BaseUrl' precisa ser definida para iniciar o Alphabit.App.");
    }

    client.BaseAddress = new Uri(configuredBaseUrl);
});
builder.Services.AddScoped<UserSessionState>();
builder.Services.AddScoped<CartState>();
builder.Services.AddScoped<AuthOverlayState>();
builder.Services.AddScoped<EventSearchState>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
