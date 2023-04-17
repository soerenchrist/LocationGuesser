using Azure.Storage.Blobs.Models;

namespace LocationGuesser.Core.Data.Abstractions;

public interface IBlobContainer
{
    Task<BlobContentInfo> UploadAsync(string filename, Stream fileStream, CancellationToken cancellationToken, bool replace = false);
}