using System.Net;
using System.Text.Json;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using PagueVeloz.Shared.Middlewares;
using Serilog;


public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
        var code = HttpStatusCode.InternalServerError; // 500 por padrão
        string error = "Internal Server Error";
        string message = "Ocorreu um erro inesperado.";

        switch (exception)
        {
            case ArgumentException argEx:
                code = HttpStatusCode.BadRequest;
                error = "Bad Request";
                message = argEx.Message;
                break;
            case KeyNotFoundException notFoundEx:
                code = HttpStatusCode.NotFound;
                error = "Not Found";
                message = notFoundEx.Message;
                break;
            case BusinessException businessEx:  // <-- nova regra
                code = (HttpStatusCode)businessEx.StatusCode;
                error = "Business Error";
                message = businessEx.Message;
                break;
        }

        var result = JsonSerializer.Serialize(new
        {
            traceId,
            status = (int)code,
            error,
            message
        });

        Log.Error(exception, "Erro capturado globalmente. TraceId: {TraceId}", traceId);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        return context.Response.WriteAsync(result);
    }

}
