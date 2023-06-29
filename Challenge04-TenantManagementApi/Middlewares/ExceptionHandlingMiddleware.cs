using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph.Models.ODataErrors;

namespace Challenge04_TenantManagementApi.Middlewares;

public class ExceptionHandlingMiddleware : IMiddleware
{
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 미들웨어가 호출되어 비즈니스 로직이 발생시키는 예외를 처리합니다.
    /// </summary>
    /// <param name="httpContext">http 요청의 정보를 가지고 있는 객체</param>
    /// <param name="next">다음 단계의 동작</param>
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
        var response = context.Response;
        response.ContentType = "application/json";

        var problemDetails = new ProblemDetails
        {
            Instance = context.Request.Path,
            Title = exception.GetType().Name,
            Detail = exception.Message,
            Status = (int)HttpStatusCode.InternalServerError,
        };

        switch (exception)
        {
            case ODataError ex:
                problemDetails.Status = ex.ResponseStatusCode;
                break;
            case UnauthorizedAccessException:
                problemDetails.Status = (int)HttpStatusCode.Unauthorized;
                break;
            case KeyNotFoundException:
            case ArgumentNullException:
                problemDetails.Status = (int)HttpStatusCode.NotFound;
                break;
            case ArgumentException:
                problemDetails.Status = (int)HttpStatusCode.BadRequest;
                break;
            default:
                _logger.LogError(exception, "서버 에러 발생");
                break;
        }

        response.StatusCode = (int)problemDetails.Status;
        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}