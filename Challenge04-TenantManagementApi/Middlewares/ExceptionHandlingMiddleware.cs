using System.Net;
using System.Text.Json;
using Microsoft.Graph.Models.ODataErrors;

namespace Challenge04_TenantManagementApi.Middlewares;

public class ExceptionHandlingMiddleware : IMiddleware
{
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext, RequestDelegate next)
    {
        try
        {
            await next(httpContext);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        var response = context.Response;
        string errorResponse;

        switch (exception)
        {
            case ODataError ex:
                response.StatusCode = ex.ResponseStatusCode;
                errorResponse = ex.Message;
                break;
            case ArgumentOutOfRangeException ex:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse = ex.Message;
                break;
            case KeyNotFoundException:
            case ArgumentNullException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse = exception.Message;
                break;
            default:
                _logger.LogError(exception, "서버 에러 발생");
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse = "Internal server error!";
                break;
        }

        var result = JsonSerializer.Serialize(new { message = errorResponse });
        await context.Response.WriteAsync(result);
    }
}