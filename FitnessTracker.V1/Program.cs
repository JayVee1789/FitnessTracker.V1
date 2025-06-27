//using Microsoft.AspNetCore.Components.Web;
//using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
//using FitnessTracker.V1;
//using Blazored.LocalStorage;
//using FitnessTracker.V1.Services;
//using FitnessTracker.V1.Services.ProgrammeGeneration;
//using Supabase;

//var builder = WebAssemblyHostBuilder.CreateDefault(args);
//builder.RootComponents.Add<App>("#app");
//builder.RootComponents.Add<HeadOutlet>("head::after");

//// Services
//builder.Services.AddBlazoredLocalStorage();
//builder.Services.AddScoped<AuthService>();
//builder.Services.AddScoped<PoidsService>();
//builder.Services.AddScoped<ProfileService>();
//builder.Services.AddHttpClient<SupabaseService>();

//builder.Services.AddSingleton<ProgrammeGeneratorService>();
//builder.Services.AddSingleton<IProgrammeStrategy, TbtProgrammeStrategy>();

//// Supabase client
//builder.Services.AddSingleton(sp =>
//{
//    var supabaseUrl = "https://zvshapdlwzzytpmvgmib.supabase.co";
//    var supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Inp2c2hhcGRsd3p6eXRwbXZnbWliIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDgxMDg5NjEsImV4cCI6MjA2MzY4NDk2MX0.DKx7CvWsfo9b5V6-vShqHXU1eNrvYXYDP26uOtEghCc";

//    var options = new SupabaseOptions
//    {
//        AutoRefreshToken = true,
//        AutoConnectRealtime = false
//    };

//    return new Client(supabaseUrl, supabaseKey, options);
//});

//// HttpClient
//builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
//builder.Logging.SetMinimumLevel(LogLevel.Debug);

//var host = builder.Build();

////  Recharger la session au démarrage
//var supabase = host.Services.GetRequiredService<Client>();
//var localStorage = host.Services.GetRequiredService<Blazored.LocalStorage.ILocalStorageService>();

//var refreshToken = await localStorage.GetItemAsync<string>("refresh_token");
//if (!string.IsNullOrEmpty(refreshToken))
//{
//    try
//    {
//        await supabase.Auth.RefreshSession();

//        Console.WriteLine(" Session Supabase restaurée");
//    }
//    catch (Exception ex)
//    {
//        Console.WriteLine(" Erreur restauration session : " + ex.Message);
//    }
//}

//await host.RunAsync();
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Blazored.LocalStorage;
using FitnessTracker.V1;
using FitnessTracker.V1.Services;
using FitnessTracker.V1.Services.ProgrammeGeneration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http;
using Supabase;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// -------------------------------------------------------------------
// 1) Configuration Supabase (URL, AnonKey, noms de tables…)
// -------------------------------------------------------------------
builder.Services.Configure<FitnessTracker.V1.Options.SupabaseOptions>(
    builder.Configuration.GetSection("Supabase"));

// -------------------------------------------------------------------
// 2) Services applicatifs usuels
// -------------------------------------------------------------------
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<PoidsService>();
builder.Services.AddScoped<ProfileService>();

builder.Services.AddSingleton<ProgrammeGeneratorService>();
builder.Services.AddSingleton<IProgrammeStrategy, TbtProgrammeStrategy>();

// -------------------------------------------------------------------
// 3) HttpClient : a) générique (site)   b) typé pour SupabaseService
// -------------------------------------------------------------------

// a) Générique → appels internes (wwwroot, API du même domaine)
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

// b) Typé → accès REST Supabase
builder.Services.AddHttpClient<SupabaseService2>((sp, client) =>
{
    var cfg = sp.GetRequiredService<IOptions<FitnessTracker.V1.Options.SupabaseOptions>>().Value;

    client.BaseAddress = new Uri($"{cfg.Url}/rest/v1/");
    client.DefaultRequestHeaders.Add("apikey", cfg.AnonKey);
});

// -------------------------------------------------------------------
// 4) Client Supabase SDK (auth, RPC, realtime…)
// -------------------------------------------------------------------
builder.Services.AddScoped(sp =>
{
    var cfg = sp.GetRequiredService<IOptions<FitnessTracker.V1.Options.SupabaseOptions>>().Value;

    var supaOpts = new Supabase.SupabaseOptions
    {
        AutoRefreshToken = true,
        AutoConnectRealtime = false
    };

    return new Client(cfg.Url, cfg.AnonKey, supaOpts);
});

// -------------------------------------------------------------------
// 5) Logging
// -------------------------------------------------------------------
builder.Logging.SetMinimumLevel(LogLevel.Debug);

var host = builder.Build();

// -------------------------------------------------------------------
// 6) 🔄 Tentative de restauration de session Supabase au démarrage
// -------------------------------------------------------------------
var supabase = host.Services.GetRequiredService<Client>();
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
        Console.WriteLine("❌ Erreur restauration session : " + ex.Message);
    }
}

await host.RunAsync();
