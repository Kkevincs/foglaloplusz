using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Foglaloplusz.BLL.Exceptions;

namespace Foglaloplusz.presentation.Filters;

/// <summary>A BLL kivételeit egységes HTTP válaszokká alakítja (SOLID: egy felelősség).</summary>
public sealed class BusinessExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var ex = context.Exception;

        switch (ex)
        {
            case AppValidationException ve:
                context.Result = new BadRequestObjectResult(ve.Message);
                context.ExceptionHandled = true;
                return;
            case AppNotFoundException nf:
                context.Result = new NotFoundObjectResult(new { message = nf.Message });
                context.ExceptionHandled = true;
                return;
            case AppForbiddenException fe:
                context.Result = new ObjectResult(new { message = fe.Message }) { StatusCode = StatusCodes.Status403Forbidden };
                context.ExceptionHandled = true;
                return;
            case UnauthorizedAccessException:
                context.Result = new ForbidResult();
                context.ExceptionHandled = true;
                return;
        }

        if (IsFromBll(ex) && ex is ArgumentException argEx)
        {
            context.Result = new BadRequestObjectResult(argEx.Message);
            context.ExceptionHandled = true;
        }
    }

    private static bool IsFromBll(Exception ex) =>
        ex.GetType().Namespace?.StartsWith("Foglaloplusz.BLL", StringComparison.Ordinal) == true;
}
