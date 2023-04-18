using FluentValidation;
using LocationGuesser.Api.Endpoints;
using LocationGuesser.Core;
using LocationGuesser.Core.Data.Abstractions;
using LocationGuesser.Core.Data.Dtos;
using LocationGuesser.Core.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
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
builder.Services.AddCoreDependencies();

var applicationInsightsConnection = builder.Configuration.GetConnectionString("APPLICATIONINSIGHTS_CONNECTION_STRING");
if (applicationInsightsConnection != null)
{
    builder.Logging.AddApplicationInsights(configureTelemetryConfiguration: config =>
    {
        config.ConnectionString = applicationInsightsConnection;
    }, configureApplicationInsightsLoggerOptions: options => { });
    builder.Services.AddApplicationInsightsTelemetry();
}

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "LocationGuesser API v1");
    });
}
app.UseStaticFiles();
app.UseHttpsRedirection();
app.MapImageSetEndpoints();
app.MapGet("/health", () => Results.Ok());

app.Run();