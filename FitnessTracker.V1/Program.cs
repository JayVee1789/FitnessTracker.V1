using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using FitnessTracker.V1;
using Blazored.LocalStorage;
using FitnessTracker.V1.Services;
using FitnessTracker.V1.Services.ProgrammeGeneration;
using Supabase;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Services
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<PoidsService>();
builder.Services.AddScoped<ProfileService>();
builder.Services.AddHttpClient<SupabaseService>();

builder.Services.AddSingleton<ProgrammeGeneratorService>();
builder.Services.AddSingleton<IProgrammeStrategy, TbtProgrammeStrategy>();

// Supabase client
builder.Services.AddSingleton(sp =>
{
    var supabaseUrl = "https://zvshapdlwzzytpmvgmib.supabase.co";
    var supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Inp2c2hhcGRsd3p6eXRwbXZnbWliIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDgxMDg5NjEsImV4cCI6MjA2MzY4NDk2MX0.DKx7CvWsfo9b5V6-vShqHXU1eNrvYXYDP26uOtEghCc";

    var options = new SupabaseOptions
    {
        AutoRefreshToken = true,
        AutoConnectRealtime = false
    };

    return new Client(supabaseUrl, supabaseKey, options);
});

// HttpClient
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

var host = builder.Build();

//  Recharger la session au démarrage
var supabase = host.Services.GetRequiredService<Client>();
var localStorage = host.Services.GetRequiredService<Blazored.LocalStorage.ILocalStorageService>();

var refreshToken = await localStorage.GetItemAsync<string>("refresh_token");
if (!string.IsNullOrEmpty(refreshToken))
{
    try
    {
        await supabase.Auth.RefreshSession();

        Console.WriteLine(" Session Supabase restaurée");
    }
    catch (Exception ex)
    {
        Console.WriteLine(" Erreur restauration session : " + ex.Message);
    }
}

await host.RunAsync();
