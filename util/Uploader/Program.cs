using System.Reflection;
using LocationGuesser.Core.Options;
using Microsoft.Extensions.Configuration;
using Uploader;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using LocationGuesser.Core;
using LocationGuesser.Core.Data.Abstractions;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", false, true)
    .AddEnvironmentVariables()
    .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
    .Build();

var services = new ServiceCollection();
services.Configure<CosmosDbOptions>(x => configuration.GetSection("Cosmos").Bind(x));
services.Configure<BlobOptions>(x => configuration.GetSection("Blob").Bind(x));
services.AddLogging(builder => builder.AddConsole());
services.AddCoreDependencies();

var serviceProvider = services.BuildServiceProvider();

var reader = new DirectoryReader();
var data = reader.ReadDirectory("images");

var blobRepository = serviceProvider.GetRequiredService<IBlobRepository>();
var imageRepository = serviceProvider.GetRequiredService<IImageRepository>();
var imageSetRepository = serviceProvider.GetRequiredService<IImageSetRepository>();

Console.WriteLine("Uploading images...");

foreach (var image in data.Files)
{
    using var stream = File.OpenRead(image.Filename);
    var filename = $"{image.SetId}/{image.Number}.jpg";
    var result = await blobRepository.UploadImageAsync(filename, stream, CancellationToken.None);
    if (result.IsFailed)
    {
        Console.WriteLine($"Failed to upload {image}: {result.Errors.First().Message}");
    }
}

Console.WriteLine("Creating image sets...");
foreach (var imageSet in data.ImageSets)
{
    var result = await imageSetRepository.AddImageSetAsync(imageSet, CancellationToken.None);
    if (result.IsFailed)
    {
        Console.WriteLine($"Failed to create image set {imageSet.Title}: {result.Errors.First().Message}");
    }
}

Console.WriteLine("Creating images...");
foreach (var image in data.Images)
{
    var result = await imageRepository.AddImageAsync(image, CancellationToken.None);
    if (result.IsFailed)
    {
        Console.WriteLine($"Failed to create image {image.Description}: {result.Errors.First().Message}");
    }
}