using FluentResults;
using LocationGuesser.Core.Domain;

namespace LocationGuesser.Blazor.Services.Abstractions;

public interface IGameApiService
{
    Task<Result<List<Image>>> GetGameSetAsync(string setSlug, int imageCount, CancellationToken cancellationToken);
}