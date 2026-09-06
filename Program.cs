using MrcDeliverySync.Components;
using MrcDeliverySync.Repositories;
using MrcDeliverySync.Services;
using MudBlazor.Services;
using Serilog;
using Microsoft.AspNetCore.DataProtection; // <-- Asegúrate de que esté arriba o dentro del bloque
var builder = WebApplication.CreateBuilder(args);


// 1. SERILOG (Va inmediatamente después de instanciar el builder)
// builder.Host.UseSerilog((context, services, configuration) => configuration
//     .ReadFrom.Configuration(context.Configuration)
//     .ReadFrom.Services(services)
//     .Enrich.FromLogContext());
// --- SERVICIOS BLAZOR NET 10 ---
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(options =>
    {
        options.DetailedErrors = true; // Permite ver el StackTrace completo del error C#
    });

// --- MUDBLAZOR ---
builder.Services.AddMudServices();

// --- REPOSITORIO DAPPER ---
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IAudioService, AudioService>();
builder.Services.AddScoped<IAuthVaultService, AuthVaultService>();
// Registrar servicio de estado global de configuración
builder.Services.AddSingleton<AppSettingsState>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var state = new AppSettingsState();

    // Asignar el valor inicial de appsettings.json
    var rawUrl = config["ApiSettings:AuthApiBaseUrl"] ?? "https://localhost:7046/";
    state.SetApiUrl(rawUrl);

    return state;
});
// --- REGISTRO DE HTTP CLIENT CON LOGGING NATIVO ---
builder.Services.AddHttpClient<IOrderRepository, OrderRepository>((serviceProvider, client) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

    var rawUrl = configuration["ApiSettings:AuthApiBaseUrl"];
    var baseUrl = !string.IsNullOrWhiteSpace(rawUrl) ? rawUrl : "https://localhost:7046/";

    if (!baseUrl.EndsWith("/"))
    {
        baseUrl += "/";
    }

    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add("X-Tunnel-Skip-Anti-Abuse-Page", "true");

    string pwaHostname = Environment.MachineName;
    string apiHost = client.BaseAddress.Host;

    // Console.WriteLine("==================================================");
    // Console.WriteLine($"[CONFIG-CHECK] PWA Hostname: {pwaHostname}");
    // Console.WriteLine($"[CONFIG-CHECK] API Base URL: {client.BaseAddress} (Host: {apiHost})");
    // Console.WriteLine("==================================================");

    logger.LogInformation("[CONFIG-CHECK] PWA Hostname: {PwaHostname} | API Host: {ApiHost} | Base URL: {BaseUrl}",
        pwaHostname, apiHost, client.BaseAddress);
});

// Registrar servicios de almacenamiento protegido para la PWA
// Registrar servicios de almacenamiento protegido para la PWA
var commonData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
var keysFolder = Path.Combine(commonData, "MrcDeliverySyncKeys");

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysFolder))
    .SetApplicationName("MrcDeliverySync");
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

// Elimina o comenta esta línea:
// app.MapStaticAssets();

// UseStaticFiles permite a IIS servir los archivos de wwwroot directamente
app.UseStaticFiles();

app.UseRouting();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();