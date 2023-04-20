using FluentResults;
using LocationGuesser.Core.Domain;

namespace LocationGuesser.Core.Services.Abstractions;

public interface IGameService
{
    Task<Result<List<Image>>> GetGameSetAsync(Guid imageSetId, int imageCount, CancellationToken cancellationToken);
}