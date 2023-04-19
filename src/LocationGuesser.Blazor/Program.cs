using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using LocationGuesser.Blazor;
using LocationGuesser.Blazor.Pages.Index;
using LocationGuesser.Blazor.Services;
using LocationGuesser.Blazor.Services.Abstractions;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMvvm();
builder.Services.AddBlazorise(options => { options.Immediate = true; }).AddBootstrapProviders()
    .AddFontAwesomeIcons();
builder.Services.AddScoped<IndexPageViewModel>();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<IImageSetApiService, ImageSetApiService>();

await builder.Build().RunAsync();