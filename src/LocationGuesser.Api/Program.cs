using FluentValidation;
using LocationGuesser.Api.Endpoints;
using LocationGuesser.Core;
using LocationGuesser.Core.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.Configure<BlobOptions>(options => builder.Configuration.Bind("Blob", options));
builder.Services.Configure<CosmosDbOptions>(options => builder.Configuration.Bind("Cosmos", options));
builder.Services.AddCoreDependencies();

var app = builder.Build();
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