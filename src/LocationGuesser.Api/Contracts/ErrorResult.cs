using System.Net.Mime;
using System.Text;
using System.Text.Json;

namespace LocationGuesser.Api.Contracts;

public class ErrorResult : IResult
{
    private readonly object? _obj;

    public ErrorResult(object? obj = null)
    {
        _obj = obj;
    }

    public async Task ExecuteAsync(HttpContext httpContext)
    {
        var content = JsonSerializer.Serialize(_obj);
        httpContext.Response.ContentType = MediaTypeNames.Application.Json;
        httpContext.Response.ContentLength = Encoding.UTF8.GetByteCount(content);
        await httpContext.Response.WriteAsync(content);
    }
}