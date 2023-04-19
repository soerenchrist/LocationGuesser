using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace LocationGuesser.Tests.Utils;

public static class TestLogger
{
    public static ILogger<T> Create<T>()
    {
        return NullLoggerFactory.Instance.CreateLogger<T>();
    }
}