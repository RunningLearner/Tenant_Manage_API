using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Challenge04_TenantManagementApi.Filters;

public class ValidateModelFilter : IAsyncActionFilter
{
    private ILogger<ValidateModelFilter>? _logger;

    /// <summary>
    /// 정의되어 있는 모델 타입에 맞지 않을 경우
    /// 예외를 발생시키고 로깅합니다.
    /// </summary>
    /// <param name="context">호출될 시점의 맥락의 정보</param>
    /// <param name="next">해당 필터의 다음에 올 작동</param>
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
