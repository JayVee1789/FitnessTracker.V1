using Blazored.LocalStorage;
using FitnessTracker.V1;
using FitnessTracker.V1.Helper;
using FitnessTracker.V1.Models;
using FitnessTracker.V1.Services;
using FitnessTracker.V1.Services.ProgrammeGeneration.ProgrammeClassic;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Options;
using Supabase;
using FT_SupabaseOptions = FitnessTracker.V1.Options.SupabaseOptions;   // 👈 alias local

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// ───────────────────────── COMPOSANTS ────────────────────────────
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazorBootstrap();

// ──────────────────── CONFIGURATION SUPABASE ─────────────────────
builder.Services.Configure<FT_SupabaseOptions>(
    builder.Configuration.GetSection("Supabase"));

// ──────────────── HttpClient « site » (wwwroot) ────────────────
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

// ───────────── Client SDK Supabase (Auth / From / RPC …) ──────────
builder.Services.AddScoped(sp =>
{
    var cfg = sp.GetRequiredService<IOptions<FT_SupabaseOptions>>().Value;
    var sdkOpts = new SupabaseOptions
    {
        AutoRefreshToken = true,
        AutoConnectRealtime = false
    };
    return new Client(cfg.Url, cfg.AnonKey, sdkOpts);
});

// ──────────────── SUPABASESERVICE & AUTRES SERVICES ───────────────
//builder.Services.AddScoped<SupabaseService>();      // ← Enregistrement du service principal Supabase
//builder.Services.AddScoped<SupabaseService2>();
builder.Services.AddScoped<ProgrammeService>();
builder.Services.AddSingleton<ProgrammeGeneratorService>();
builder.Services.AddSingleton<IProgrammeStrategy, TbtProgrammeStrategy>();
builder.Services.AddScoped<ViewSessionHelper>();
builder.Services.AddSingleton<AppState>();
// ───────── HttpClient typé pour SupabaseService2 (REST v1) ───────
builder.Services.AddHttpClient<SupabaseService2>((sp, http) =>
{
    var cfg = sp.GetRequiredService<IOptions<FT_SupabaseOptions>>().Value;
    http.BaseAddress = new Uri($"{cfg.Url}/rest/v1/");
    http.DefaultRequestHeaders.Add("apikey", cfg.AnonKey);
});

// ───────────────────── SERVICES MÉTIER ───────────────────────────
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<PoidsService>();
builder.Services.AddScoped<ProfileService>();

// ────────────────────────── LOGGING ──────────────────────────────
builder.Logging.SetMinimumLevel(LogLevel.Debug);

var host = builder.Build();

// ─────── Restaure la session Supabase si un refresh_token existe ─
var supabase = host.Services.GetRequiredService<Client>();
await supabase.InitializeAsync();

var localStorage = host.Services.GetRequiredService<ILocalStorageService>();
var refreshToken = await localStorage.GetItemAsync<string>("refresh_token");
if (!string.IsNullOrEmpty(refreshToken))
{
    try
    {
        await supabase.Auth.RefreshSession();
        Console.WriteLine("✅ Session Supabase restaurée");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Erreur restauration session : {ex.Message}");
    }
}

await host.RunAsync();
