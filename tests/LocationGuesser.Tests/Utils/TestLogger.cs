using Microsoft.Extensions.Logging;

namespace LocationGuesser.Tests.Utils;

public static class TestLogger
{
    public static ILogger<T> Create<T>() 
    {
        return Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance.CreateLogger<T>();
    }
}