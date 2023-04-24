using System.Diagnostics;

namespace LocationGuesser.Api.Middleware;

public class TraceMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TraceMiddleware> _logger;
    public TraceMiddleware(RequestDelegate next,
        ILogger<TraceMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var request = context.Request;
        var requestPath = request.Path;
        var requestMethod = request.Method;
        var stopwatch = Stopwatch.StartNew();

        _logger.LogTrace("Request {requestMethod} {requestPath} started", requestMethod, requestPath);

        await _next(context);

        _logger.LogTrace("Request {requestMethod} {requestPath} finished in {elapsed}ms", requestMethod, requestPath, stopwatch.ElapsedMilliseconds);
    }
}