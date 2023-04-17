using Azure;
using FluentResults;
using LocationGuesser.Core.Data.Abstractions;

namespace LocationGuesser.Core.Data;

public class BlobRepository : IBlobRepository
{
    private IBlobContainer _container;

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
}