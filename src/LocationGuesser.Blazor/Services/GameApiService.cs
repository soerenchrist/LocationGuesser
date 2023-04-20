using System.Net;
using System.Text.Json;
using FluentResults;
using LocationGuesser.Blazor.Services.Abstractions;
using LocationGuesser.Core.Domain;
using LocationGuesser.Core.Domain.Errors;

namespace LocationGuesser.Blazor.Services;

public class GameApiService : IGameApiService
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private readonly HttpClient _httpClient;
    public GameApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Result<List<Image>>> GetGameSetAsync(Guid setId, int imageCount, CancellationToken cancellationToken)
    {
        var url = $"/api/games/{setId}?imageCount={imageCount}";
        var response = await _httpClient.GetAsync(url, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await MapSuccess(response, cancellationToken);
        }

        return MapError(response.StatusCode);
    }

    private async Task<Result<List<Image>>> MapSuccess(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var images = JsonSerializer.Deserialize<List<Image>>(json, _jsonSerializerOptions);
            if (images == null) throw new JsonException("Invalid json");
            return Result.Ok(images);
        }
        catch (JsonException)
        {
            return Result.Fail<List<Image>>("Unexpected error");
        }
    }

    private Result<List<Image>> MapError(HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.NotFound => Result.Fail<List<Image>>(new NotFoundError("Game set not found")),
            _ => Result.Fail<List<Image>>("Unexpected error")
        };
    }

    public string GetImageContentUrl(Guid setId, int number)
    {
        var url = $"/api/game/{setId}/image/{number}/content";

        return url;
    }
}