using Microsoft.AspNetCore.Http;
using Serilog;
using System.Diagnostics;

namespace PagueVeloz.Shared.Middlewares
{
    public class RequestLog
    {
        private readonly RequestDelegate _next;

        public RequestLog(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var sw = Stopwatch.StartNew();

            var correlationId = Guid.NewGuid();
            context.Items["CorrelationId"] = correlationId;

            Log.Information("[REQUEST] {Method} {Path} CorrelationId={CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                correlationId);

            await _next(context);

            sw.Stop();

            Log.Information("[RESPONSE] {Method} {Path} {Status} {Elapsed}ms CorrelationId={CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                sw.ElapsedMilliseconds,
                correlationId);
        }
    }
}
