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

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var requestIp = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
        _logger.LogDebug("[{ClassName}] 다음 주소로부터 헬스체크 요청 수신받음 - {RequestIp}", ClassName, requestIp);
        return Task.FromResult(HealthCheckResult.Healthy());
    }
}
