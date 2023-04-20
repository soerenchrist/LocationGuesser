using FluentValidation;
using LocationGuesser.Api.Endpoints;
using LocationGuesser.Core;
using LocationGuesser.Core.Options;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "LocationGuesser API", Version = "v1" });
});

builder.Services.AddOptions<BlobOptions>()
    .BindConfiguration("Blob")
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddOptions<CosmosDbOptions>()
    .BindConfiguration("Cosmos")
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddMediatR(config => config.RegisterServicesFromAssemblyContaining<Program>());
var useInMemory = builder.Configuration.GetValue("UseInMemory", false);
builder.Services.AddCoreDependencies(useInMemory);

var applicationInsightsConnection = builder.Configuration.GetConnectionString("APPLICATIONINSIGHTS_CONNECTION_STRING");
if (applicationInsightsConnection != null)
{
    builder.Logging.AddApplicationInsights(config => { config.ConnectionString = applicationInsightsConnection; },
        options => { });
    builder.Services.AddApplicationInsightsTelemetry();
}

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseSwagger();
    app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "LocationGuesser API v1"); });
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.UseRouting();

app.MapFallbackToFile("index.html");

app.MapImageSetEndpoints();
app.MapGameEndpoints();
app.MapGet("/health", () => Results.Ok());

app.Run();