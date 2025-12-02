using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

public class RequestTimingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestTimingMiddleware> _logger;

    public RequestTimingMiddleware(RequestDelegate next, ILogger<RequestTimingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();
        var traceId = context.TraceIdentifier;

        _logger.LogInformation("Request started: {Method} {Path} TraceId={TraceId}",
            context.Request.Method, context.Request.Path, traceId);

        await _next(context);

        watch.Stop();
        _logger.LogInformation("Request finished: {Method} {Path} StatusCode={StatusCode} TraceId={TraceId} Elapsed={Elapsed}ms",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            traceId,
            watch.ElapsedMilliseconds);
    }
}
