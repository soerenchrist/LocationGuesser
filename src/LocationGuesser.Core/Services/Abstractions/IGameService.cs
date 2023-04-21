using FluentResults;
using LocationGuesser.Core.Domain;

namespace LocationGuesser.Core.Services.Abstractions;

public interface IGameService
{
    Task<Result<List<Image>>> GetGameSetAsync(string imageSetSlug, int imageCount, CancellationToken cancellationToken);
}