using System.Globalization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BookingSystem.WebUI;
using BookingSystem.WebUI.Services;
using Syncfusion.Blazor;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Русская локаль для всего приложения (месяцы, дни недели, форматы дат).
var ru = new CultureInfo("ru-RU");
CultureInfo.DefaultThreadCurrentCulture = ru;
CultureInfo.DefaultThreadCurrentUICulture = ru;
CultureInfo.CurrentCulture = ru;
CultureInfo.CurrentUICulture = ru;

// Базовый адрес WebApi (по умолчанию — локальный API на :5265).
var apiBase = builder.Configuration["ApiBaseUrl"];
if (string.IsNullOrWhiteSpace(apiBase))
    apiBase = builder.HostEnvironment.BaseAddress;

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBase) });
builder.Services.AddScoped<BookingApi>();
builder.Services.AddSyncfusionBlazor();

// Бесплатная Community-лицензия Syncfusion (если ключ задан в appsettings.json).
var sfKey = builder.Configuration["SyncfusionLicenseKey"];
if (!string.IsNullOrWhiteSpace(sfKey))
    Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(sfKey);

await builder.Build().RunAsync();
