using LocationGuesser.Blazor.Services;
using RichardSzalay.MockHttp;

namespace LocationGuesser.Blazor.Tests.Services;

public class ImageSetApiServiceTests
{
    private readonly ImageSetApiService _cut;
    private MockHttpMessageHandler _mockHttpMessageHandler = new();
    public ImageSetApiServiceTests()
    {
        var client = _mockHttpMessageHandler.ToHttpClient();
        _cut = new ImageSetApiService(client);
    }
}