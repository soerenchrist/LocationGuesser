using FluentValidation;
using LocationGuesser.Api.Features.Game;
using LocationGuesser.Api.Features.ImageSets;
using LocationGuesser.Api.Middleware;
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

builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(builder => builder.Expire(TimeSpan.FromMinutes(5)));
    options.AddPolicy("NoCache", builder => builder.NoCache());
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddMediatR(config => config.RegisterServicesFromAssemblyContaining<Program>());
builder.Services.AddCoreDependencies();

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
    app.UseSwagger();
    app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "LocationGuesser API v1"); });
    app.UseCors("AllowAll");
}

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionHandlerMiddleware>();
app.UseMiddleware<TraceMiddleware>();
app.UseOutputCache();

app.MapImageSetEndpoints();
app.MapGameEndpoints();

app.MapGet("/health", () => Results.Ok());

app.Run();