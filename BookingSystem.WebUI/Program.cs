using System.Globalization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BookingSystem.WebUI;
using BookingSystem.WebUI.Services;
using Syncfusion.Blazor;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var ru = new CultureInfo("ru-RU");
CultureInfo.DefaultThreadCurrentCulture = ru;
CultureInfo.DefaultThreadCurrentUICulture = ru;
CultureInfo.CurrentCulture = ru;
CultureInfo.CurrentUICulture = ru;

var apiBase = builder.Configuration["ApiBaseUrl"];
if (string.IsNullOrWhiteSpace(apiBase))
    apiBase = builder.HostEnvironment.BaseAddress;

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBase) });
builder.Services.AddScoped<BookingApi>();
builder.Services.AddSyncfusionBlazor();

var sfKey = builder.Configuration["SyncfusionLicenseKey"];
if (!string.IsNullOrWhiteSpace(sfKey))
    Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(sfKey);

await builder.Build().RunAsync();
