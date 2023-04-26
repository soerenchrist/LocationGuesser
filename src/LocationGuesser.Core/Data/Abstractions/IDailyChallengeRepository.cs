using FluentResults;
using LocationGuesser.Core.Domain;

namespace LocationGuesser.Core.Data.Abstractions;

public interface IDailyChallengeRepository
{
    Task<Result<DailyChallenge>> GetDailyChallengeAsync(DateTime date, CancellationToken cancellationToken);

    Task<Result<DailyChallenge>> AddDailyChallengeAsync(DailyChallenge dailyChallenge,
        CancellationToken cancellationToken);
}