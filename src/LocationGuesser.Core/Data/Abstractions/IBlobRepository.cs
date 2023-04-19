using FluentResults;

namespace LocationGuesser.Core.Data.Abstractions;

public interface IBlobRepository
{
    Task<Result> UploadImageAsync(string filename, Stream fileStream, CancellationToken cancellationToken);
    Task<Result> DeleteImageAsync(string filename, CancellationToken cancellationToken);
}