using LocationGuesser.Core.Options;
using LocationGuesser.Core.Services.Abstractions;
using Microsoft.Extensions.Options;

namespace LocationGuesser.Core.Services;

public class ImageUrlService : IImageUrlService
{
    private readonly string _endpoint;
    public ImageUrlService(IOptions<BlobOptions> options)
    {
        _endpoint = options.Value.Endpoint;
    }

    public string GetImageUrl(Guid setId, int number)
    {
        if (_endpoint.EndsWith("/"))
        {
            return $"{_endpoint}images/{setId}/{number}.jpg";
        }

        return $"{_endpoint}/images/{setId}/{number}.jpg";
    }
}