using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AssignmentSystem.Api.Middleware;

/// <summary>
/// Resolves an IValidator&lt;T&gt; for each action argument (if registered) and validates it before the
/// action runs, throwing FluentValidation.ValidationException on failure — handled centrally by
/// ExceptionHandlingMiddleware, so controllers stay free of manual validation calls.
/// </summary>
public class ValidationActionFilter : IAsyncActionFilter
{
    private readonly IServiceProvider _serviceProvider;

    public ValidationActionFilter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null) continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (_serviceProvider.GetService(validatorType) is IValidator validator)
            {
                var validationContext = new ValidationContext<object>(argument);
                var result = await validator.ValidateAsync(validationContext);
                if (!result.IsValid)
                {
                    throw new ValidationException(result.Errors);
                }
            }
        }

        await next();
    }
}
