using Azure;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using LocationGuesser.Core.Data.Abstractions;
using LocationGuesser.Core.Options;
using Microsoft.Extensions.Options;

namespace LocationGuesser.Core.Data;

public class AzureBlobContainer : IBlobContainer
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly BlobContainerClient _containerClient;

    public AzureBlobContainer(IOptions<BlobOptions> blobOptions)
    {
        var options = blobOptions.Value;
        _blobServiceClient = new BlobServiceClient(new Uri(options.Endpoint), new DefaultAzureCredential());
        _containerClient = _blobServiceClient.GetBlobContainerClient(options.ContainerName);
    }

    public Task DeleteAsync(string filename, CancellationToken cancellationToken)
    {
        var client = _containerClient.GetBlobClient(filename);
        return client.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    public async Task<Stream?> DownloadContentAsync(string filename, CancellationToken cancellationToken)
    {
        var client = _containerClient.GetBlobClient(filename);
        var result = await client.DownloadContentAsync(cancellationToken: cancellationToken);
        return result.GetRawResponse().ContentStream;
    }

    public async Task<BlobContentInfo> UploadAsync(string filename, Stream fileStream,
        CancellationToken cancellationToken, bool replace = false)
    {
        var client = _containerClient.GetBlobClient(filename);
        return await client.UploadAsync(fileStream, cancellationToken: cancellationToken, overwrite: replace);
    }
}