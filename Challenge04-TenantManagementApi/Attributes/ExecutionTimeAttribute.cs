using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Challenge04_TenantManagementApi.Attributes;

[AttributeUsage(validOn: AttributeTargets.Method)]
public class ExecutionTimeAttribute : Attribute, IAsyncActionFilter
{
    private ILogger<ExecutionTimeAttribute>? _logger;

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
