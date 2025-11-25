namespace ASP.Claims.API.Middleware.Filters;

using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class FluentValidationActionFilter(IServiceProvider serviceProvider) : IActionFilter
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public void OnActionExecuting(ActionExecutingContext context)
    {
        foreach (var arg in context.ActionArguments.Values)
        {
            if (arg == null) continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(arg.GetType());

            if (_serviceProvider.GetService(validatorType) is IValidator validator)
            {
                var validationContext = new ValidationContext<object>(arg);
                var result = validator.Validate(validationContext);
                if (!result.IsValid)
                {
                    context.Result = new BadRequestObjectResult(result.Errors.Select(e => new
                    {
                        e.PropertyName,
                        e.ErrorMessage
                    }));
                    return;
                }
            }
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // No-op
    }
}