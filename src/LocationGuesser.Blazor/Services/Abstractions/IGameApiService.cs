using FluentResults;
using LocationGuesser.Core.Domain;

namespace LocationGuesser.Blazor.Services.Abstractions;

public interface IGameApiService
{
    Task<Result<List<Image>>> GetGameSetAsync(Guid setId, int imageCount, CancellationToken cancellationToken);
    string GetImageContentUrl(Guid setId, int number);
}