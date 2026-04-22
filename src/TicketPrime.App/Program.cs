using TicketPrime.App.Components;
using TicketPrime.App.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddHttpClient<TicketPrimeApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["TicketPrimeApi:BaseUrl"] ?? "http://localhost:5238/");
});
builder.Services.AddScoped<UserSessionState>();
builder.Services.AddScoped<CartState>();
builder.Services.AddScoped<AuthOverlayState>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
