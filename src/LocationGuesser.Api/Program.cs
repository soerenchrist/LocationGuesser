using LocationGuesser.Core;
using LocationGuesser.Core.Data.Abstractions;
using LocationGuesser.Core.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<BlobOptions>(options => builder.Configuration.Bind("Blob", options));
builder.Services.Configure<CosmosDbOptions>(options => builder.Configuration.Bind("Cosmos", options));
builder.Services.AddCoreDependencies();

builder.Services.AddGrpc();
var app = builder.Build();
/*

// Configure the HTTP request pipeline.
app.MapGrpcService<ImageSetGrpcService>();
app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
*/