using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Hr.PL.Filters
{
    /// <summary>
    /// A globally registered action filter that validates any DTO in the action arguments
    /// using the registered FluentValidation validator for that type.
    /// </summary>
    public class GlobalValidationFilter(IServiceProvider serviceProvider) : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            foreach (var argument in context.ActionArguments.Values)
            {
                if (argument is null) continue;

                var dtoType = argument.GetType();

                // Try to resolve IValidator<TDto> from DI for this argument's type
                var validatorType = typeof(IValidator<>).MakeGenericType(dtoType);
                var validator = serviceProvider.GetService(validatorType) as IValidator;

                if (validator is null) continue;

                var validationContext = new ValidationContext<object>(argument);
                var result = await validator.ValidateAsync(validationContext, context.HttpContext.RequestAborted);

                if (!result.IsValid)
                {
                    var errors = result.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray());

                    context.Result = new BadRequestObjectResult(new ValidationProblemDetails(errors));
                    return;
                }
            }

            await next();
        }
    }
}
