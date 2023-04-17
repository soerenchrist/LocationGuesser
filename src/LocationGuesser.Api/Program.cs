using FluentValidation;
using LocationGuesser.Api.Endpoints;
using LocationGuesser.Core;
using LocationGuesser.Core.Options;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "LocationGuesser API", Version = "v1" });
});

builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.Configure<BlobOptions>(options => builder.Configuration.Bind("Blob", options));
builder.Services.Configure<CosmosDbOptions>(options => builder.Configuration.Bind("Cosmos", options));
builder.Services.AddCoreDependencies();

var applicationInsightsConnection = builder.Configuration.GetConnectionString("APPLICATIONINSIGHTS_CONNECTION_STRING");
if (applicationInsightsConnection != null)
{
    builder.Logging.AddApplicationInsights(configureTelemetryConfiguration: config =>
    {
        config.ConnectionString = applicationInsightsConnection;
    }, configureApplicationInsightsLoggerOptions: options => { });
}

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "LocationGuesser API v1");
});
app.UseStaticFiles();
app.MapImageSetEndpoints();

app.Run();
/*

// Configure the HTTP request pipeline.
app.MapGrpcService<ImageSetGrpcService>();
app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
*/