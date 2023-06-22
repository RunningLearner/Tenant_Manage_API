using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Challenge04_TenantManagementApi.Filters;

public class ValidateModelFilter : IAsyncActionFilter
{
    private ILogger<ValidateModelFilter>? _logger;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        _logger ??= context.HttpContext.RequestServices.GetRequiredService<ILogger<ValidateModelFilter>>();
        var actionName = context.ActionDescriptor.DisplayName;

        if (!context.ModelState.IsValid)
        {
            context.Result = new BadRequestObjectResult(context.ModelState);
            _logger.LogError("유효하지 않은 모델이 입력되었습니다. {ActionName}", actionName);
            return;
        }

        await next();
    }
}
