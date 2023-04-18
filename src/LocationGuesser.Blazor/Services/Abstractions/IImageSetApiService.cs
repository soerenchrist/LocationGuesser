using LocationGuesser.Api.Contracts;

namespace LocationGuesser.Blazor.Services.Abstractions;

public interface IImageSetApiService
{
    Task<ImageSetContract> ListImageSetsAsync(CancellationToken cancellationToken = default);
}