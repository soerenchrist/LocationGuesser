using LocationGuesser.Api.Services;
using LocationGuesser.Core.Data;
using LocationGuesser.Core.Data.Abstractions;
using LocationGuesser.Core.Domain;
using LocationGuesser.Core.Options;
using LocationGuesser.Core.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<CosmosDbOptions>(options => builder.Configuration.Bind("Cosmos", options));
builder.Services.AddScoped<IImageSetRepository, CosmosImageSetRepository>();
builder.Services.AddScoped<ImageSetService>();

builder.Services.AddGrpc();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();
app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

using (var scope = app.Services.CreateScope())
{
    var repository = scope.ServiceProvider.GetRequiredService<IImageSetRepository>();
    await repository.AddImageSetAsync(new ImageSet(Guid.NewGuid(), "TestTitle", "TestDescription", "TestTags"),
        default);
}

app.Run();