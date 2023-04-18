using System.Net;
using System.Text.Json;
using FluentResults;
using LocationGuesser.Blazor.Mappings;
using LocationGuesser.Blazor.Services.Abstractions;
using LocationGuesser.Core.Contracts;
using LocationGuesser.Core.Domain;
using LocationGuesser.Core.Domain.Errors;

namespace LocationGuesser.Blazor.Services;

public class ImageSetApiService : IImageSetApiService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    public ImageSetApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Result<List<ImageSet>>> ListImageSetsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("/api/imagesets", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return response.StatusCode switch
            {
                HttpStatusCode.NotFound => Result.Fail(new NotFoundError("Resource not found")),
                _ => Result.Fail("Something went wrong"),
            };
        }

        try
        {
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var imageSets = JsonSerializer.Deserialize<List<ImageSetContract>>(json, _jsonOptions);
            if (imageSets == null) throw new JsonException("Invalid json");

            return Result.Ok(imageSets.Select(x => x.ToDomain()).ToList());
        }
        catch (JsonException)
        {
            return Result.Fail("Something went wrong");
        }
    }
}