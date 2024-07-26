using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Challenge04_TenantManagementApi.Attributes;

[AttributeUsage(validOn: AttributeTargets.Class | AttributeTargets.Method)]
public class ApiKeyAuthAttribute : Attribute, IAsyncActionFilter
{
    private IConfiguration? _configuration;
    private ILogger<ApiKeyAuthAttribute>? _logger;

    /// <summary>
    /// 해당 Attribute를 가진 메서드나 클래스는 작동 전
    /// 요청이 API 키를 가지고 있는지 검증한다.
    /// </summary>
    /// <param name="context">호출될 시점의 맥락의 정보</param>
    /// <param name="next">해당 필터의 다음에 올 작동</param>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        _configuration ??= context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        _logger ??= context.HttpContext.RequestServices.GetRequiredService<ILogger<ApiKeyAuthAttribute>>();

        // 환경설정의 API Key 값을 읽어온다. 만약 환경설정을 가져오지 않았다면 가져온다.
        var apiKey = _configuration.GetValue<string>("Secret:ApiKey");

        // API Key 인증을 사용하도록 설정된 경우에만 동작
        if (!string.IsNullOrEmpty(apiKey))
        {
            // 인증 헤더가 제공되지 않은 경우 UnauthorizedAccessException 예외 발생
            if (!context.HttpContext.Request.Headers.TryGetValue("X-API-KEY", out var authHeaderValue))
            {
                _logger.LogError("Unauthorized - API KEY가 제공되지 않음");
                throw new UnauthorizedAccessException("오류: API KEY가 제공되지 않았습니다.");
            }

            // HTTP 요청 헤더에 제공된 API Key가 일치하지 않는 경우 UnauthorizedAccessException 예외 발생
            if (!apiKey.Equals(authHeaderValue))
            {
                _logger.LogError("Unauthorized - API KEY가 유효하지 않음, X-API-KEY: {ApiKeyHeaderValue}", authHeaderValue!);
                throw new UnauthorizedAccessException("오류: API KEY가 유효하지 않습니다.");
            }
        }

        // API Key가 일치하면 다음 처리를 계속 진행한다.
        await next();
    }
}
