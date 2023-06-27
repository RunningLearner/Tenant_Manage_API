using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Challenge04_TenantManagementApi.Attributes;

[AttributeUsage(validOn: AttributeTargets.Method)]
public class ExecutionTimeAttribute : Attribute, IAsyncActionFilter
{
    private ILogger<ExecutionTimeAttribute>? _logger;

    /// <summary>
    /// 해당 Attribute를 가진 메서드가 시작되기 전 타이머를 작동한다.
    /// 메서드가 작업을 완료하면 타이머의 시간을 로깅한다.
    /// </summary>
    /// <param name="context">호출될 시점의 맥락의 정보</param>
    /// <param name="next">해당 필터의 다음에 올 작동</param>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        _logger ??= context.HttpContext.RequestServices.GetRequiredService<ILogger<ExecutionTimeAttribute>>();
        var actionName = context.ActionDescriptor.DisplayName;
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        await next();

        stopwatch.Stop();

        _logger.LogInformation("Action {ActionName} Execution Time: {Duration:F3} seconds", actionName, stopwatch.Elapsed.TotalSeconds);
    }
}
