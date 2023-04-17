using FluentResults;
using LocationGuesser.Core.Domain;

namespace LocationGuesser.Core.Data.Abstractions;

public interface IImageRepository
{
    Task<Result<Image?>> GetImageAsync(Guid setId, int number, CancellationToken cancellationToken);
}