using System.Text.Json;

namespace LocationGuesser.Blazor.Tests.Helpers;

public static class JsonHelper
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    public static string ToJson<T>(T data)
    {
        var json = JsonSerializer.Serialize(data, _jsonSerializerOptions);
        return json;
    }
}