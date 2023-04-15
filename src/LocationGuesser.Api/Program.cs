using Azure.Identity;
using LocationGuesser.Api.Services;
using LocationGuesser.Core.Domain;
using Microsoft.Azure.Cosmos;

var builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.
var endpoint = builder.Configuration.GetValue("CosmosEndpoint", "");
if (endpoint == string.Empty) throw new Exception("Invalid endpoint");
using var client = new CosmosClient(
    endpoint, tokenCredential: new DefaultAzureCredential());

var db = client.GetDatabase("LocationGuesser");
var container = db.GetContainer("Items");

var item = new CosmosImageSet
{
    Id = Guid.NewGuid(),
    Title = "Test",
    Description = "Desc",
    Tags = "Test123"
};
await container.CreateItemAsync(item, partitionKey: new PartitionKey(item.Id.ToString()));

builder.Services.AddGrpc();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();
app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();