using System.Text.Json;
using LocationGuesser.Api.Contracts;

namespace LocationGuesser.Blazor.Services;

public class ImageSetApiService
{
    private readonly HttpClient _httpClient;
    public ImageSetApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ImageSetContract> ListImageSetsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("api/imagesets", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Unable to list image sets");
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<ImageSetContract>(content)!;
    }
}