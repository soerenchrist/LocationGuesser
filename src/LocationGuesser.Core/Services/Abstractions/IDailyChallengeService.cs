using FluentResults;
using LocationGuesser.Core.Domain;

namespace LocationGuesser.Core.Services.Abstractions;

public interface IDailyChallengeService
{
    Task<Result<DailyChallenge>> GetDailyChallengeAsync(DateTime dateTime,
        CancellationToken cancellationToken);
}