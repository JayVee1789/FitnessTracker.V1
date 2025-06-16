using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using FitnessTracker.V1;
using Blazored.LocalStorage;
using FitnessTracker.V1.Services;
using FitnessTracker.V1.Services.ProgrammeGeneration;




var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<PoidsService>();
builder.Services.AddHttpClient<SupabaseService>();

builder.Services.AddSingleton<ProgrammeGeneratorService>();
builder.Services.AddSingleton<IProgrammeStrategy, TbtProgrammeStrategy>();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();
