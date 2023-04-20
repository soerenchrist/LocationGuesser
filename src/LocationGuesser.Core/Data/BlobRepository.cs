using Azure;
using FluentResults;
using LocationGuesser.Core.Data.Abstractions;
using LocationGuesser.Core.Domain.Errors;

namespace LocationGuesser.Core.Data;

public class BlobRepository : IBlobRepository
{
    private readonly IBlobContainer _container;

    public BlobRepository(IBlobContainer container)
    {
        _container = container;
    }

    public async Task<Result> DeleteImageAsync(string filename, CancellationToken cancellationToken)
    {
        try
        {
            await _container.DeleteAsync(filename, cancellationToken);
        }
        catch (RequestFailedException ex)
        {
            return Result.Fail(ex.Message);
        }

        return Result.Ok();
    }

    public async Task<Result> UploadImageAsync(string filename, Stream fileStream, CancellationToken cancellationToken)
    {
        try
        {
            await _container.UploadAsync(filename, fileStream, cancellationToken, true);
        }
        catch (RequestFailedException ex)
        {
            return Result.Fail(ex.Message);
        }

        return Result.Ok();
    }

    public async Task<Result<Stream>> DownloadImageAsync(string filename, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _container.DownloadContentAsync(filename, cancellationToken);
            if (response is null) return Result.Fail<Stream>(new NotFoundError("File not found"));
            return Result.Ok(response);
        }
        catch (RequestFailedException ex)
        {
            return Result.Fail<Stream>(ex.Message);
        }
    }
}