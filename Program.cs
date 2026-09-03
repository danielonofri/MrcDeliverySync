using MrcDeliverySync.Components;
using MrcDeliverySync.Repositories;
using MrcDeliverySync.Services;
using MudBlazor.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);


// 1. SERILOG (Va inmediatamente después de instanciar el builder)
// builder.Host.UseSerilog((context, services, configuration) => configuration
//     .ReadFrom.Configuration(context.Configuration)
//     .ReadFrom.Services(services)
//     .Enrich.FromLogContext());
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

// En .NET 10 MapStaticAssets debe ir para activos optimizados
app.MapStaticAssets();

// UseStaticFiles permite a IIS servir los archivos de wwwroot directamente
app.UseStaticFiles();

app.UseRouting();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();