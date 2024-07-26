using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Challenge04_TenantManagementApi.Services;

public class HealthCheckService : IHealthCheck
{
    private const string ClassName = nameof(HealthCheckService);
    private readonly ILogger<HealthCheckService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HealthCheckService(ILogger<HealthCheckService> logger, IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// 앱이 정상적으로 작동하고 있는지 확인하는 메서드
    /// 요청 IP를 로깅합니다.
    /// </summary>
    /// <param name="context">실행중인 헬스체크에 대한 정보</param>
    /// <param name="cancellationToken">중단시킬 수 있는 토큰</param>
    /// <returns></returns>
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var requestIp = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
        _logger.LogDebug("[{ClassName}] 다음 주소로부터 헬스체크 요청 수신받음 - {RequestIp}", ClassName, requestIp);
        return Task.FromResult(HealthCheckResult.Healthy());
    }
}
