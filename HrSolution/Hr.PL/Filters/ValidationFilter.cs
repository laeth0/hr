using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Hr.PL.Filters
{
    public class ValidationFilter<TDto>(IValidator<TDto> validator) : IAsyncActionFilter
        where TDto : class
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var dto = context.ActionArguments.Values.OfType<TDto>().FirstOrDefault();

            if (dto is null)
            {
                await next();
                return;
            }

            var result = await validator.ValidateAsync(dto, context.HttpContext.RequestAborted);

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

            await next();
        }
    }
}
