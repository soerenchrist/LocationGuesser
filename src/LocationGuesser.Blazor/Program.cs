using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using LocationGuesser.Blazor;
using LocationGuesser.Blazor.Services.Abstractions;
using LocationGuesser.Blazor.Services;
using LocationGuesser.Blazor.Pages.Index;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMvvm();
builder.Services.AddScoped<IndexPageViewModel>();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<IImageSetApiService, ImageSetApiService>();

await builder.Build().RunAsync();
