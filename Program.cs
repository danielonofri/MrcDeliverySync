using MrcDeliverySync.Components;
using MrcDeliverySync.Repositories;
using MrcDeliverySync.Services;
using MudBlazor.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();
// --- SERVICIOS BLAZOR NET 10 ---
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// --- MUDBLAZOR ---
builder.Services.AddMudServices();

// --- REPOSITORIO DAPPER ---
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IAudioService, AudioService>();
builder.Services.AddScoped<IAuthVaultService, AuthVaultService>();

builder.Services.AddHttpClient<AuthApiClient>(client =>
{
    var baseUrl = builder.Configuration["ApiSettings:AuthApiBaseUrl"]
                  ?? "https://mrctablet.com/PedidosYaApi/";

    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add("X-Tunnel-Skip-Anti-Abuse-Page", "true");
});

// Registrar servicios de almacenamiento protegido para la PWA
builder.Services.AddDataProtection();
// Forzar un Shutdown rápido (2 segundos máximo de espera para circuitos WebSocket)
builder.Services.Configure<HostOptions>(options =>
{
    options.ShutdownTimeout = TimeSpan.FromSeconds(2);
});
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UsePathBase("/MrcDeliverySync");
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

// --- MIDDLEWARES Y ARCHIVOS ESTÁTICOS ---
app.UseHttpsRedirection();

// 1. Archivos físicos planos de wwwroot (css, js, etc.)
app.UseStaticFiles();

// 2. Mapeo de activos compilados de .NET 10
app.MapStaticAssets();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();