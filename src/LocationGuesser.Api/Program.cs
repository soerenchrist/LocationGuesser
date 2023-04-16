using LocationGuesser.Api.Services;
using LocationGuesser.Core.Options;
using LocationGuesser.Core.Services;
using LocationGuesser.Core.Services.Abstractions;

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
    await repository.GetImageSetAsync(Guid.NewGuid(), default);
}

app.Run();