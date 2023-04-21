using LocationGuesser.Core.Options;
using LocationGuesser.Core.Services;
using Microsoft.Extensions.Options;

namespace LocationGuesser.Tests.Services;

public class ImageUrlServiceTests
{
    [Fact]
    public void GetImageUrl_ShouldReturnCorrectUrl()
    {
        var options = new BlobOptions
        {
            Endpoint = "https://test.blob.core.windows.net",
        };
        var cut = new ImageUrlService(Options.Create(options));
        var guid = Guid.NewGuid();
        var url = cut.GetImageUrl(guid, 1);

        url.Should().Be($"https://test.blob.core.windows.net/images/{guid}/1.jpg");
    }
    
    [Fact]
    public void GetImageUrl_ShouldReturnCorrectUrl_WhenEndsWithSlash()
    {
        var options = new BlobOptions
        {
            Endpoint = "https://test.blob.core.windows.net/",
        };
        var cut = new ImageUrlService(Options.Create(options));
        var guid = Guid.NewGuid();
        var url = cut.GetImageUrl(guid, 1);

        url.Should().Be($"https://test.blob.core.windows.net/images/{guid}/1.jpg");
    }
}