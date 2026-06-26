using Hr.BLL.Common;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Hr.PL.Controllers
{
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        protected string? CurrentUserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        protected string? CurrentUserEmail => User.FindFirst(ClaimTypes.Email)?.Value;

        protected IActionResult OkOrProblem<T>(Result<T> result)
            => result.IsSuccess ? Ok(result.Value) : MapError(result.Error);

        protected IActionResult NoContentOrProblem(Result result)
            => result.IsSuccess ? NoContent() : MapError(result.Error);

        protected IActionResult MapError(Error error) => error.Type switch
        {
            ErrorType.NotFound => NotFound(new ProblemDetails
            {
                Title = error.Code,
                Detail = error.Message,
                Status = StatusCodes.Status404NotFound
            }),
            ErrorType.Conflict => Conflict(new ProblemDetails
            {
                Title = error.Code,
                Detail = error.Message,
                Status = StatusCodes.Status409Conflict
            }),
            ErrorType.Validation => BadRequest(new ValidationProblemDetails(
                new Dictionary<string, string[]> { [""] = error.Details?.ToArray() ?? [] })
            {
                Status = StatusCodes.Status400BadRequest
            }),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = error.Code,
                Detail = error.Message,
                Status = StatusCodes.Status500InternalServerError
            })
        };
    }
}
