using FluentResults;
using LocationGuesser.Core.Domain;

namespace LocationGuesser.Blazor.Services.Abstractions;

public interface IImageSetApiService
{
    Task<Result<List<ImageSet>>> ListImageSetsAsync(CancellationToken cancellationToken = default);
}