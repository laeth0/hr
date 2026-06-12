using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Hr.PL.Controllers
{
    /// <summary>
    /// Base controller for all API controllers in the application.
    /// Provides common routing, API behavioral attributes, and helper properties.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseController : ControllerBase
    {
        /// <summary>
        /// Retrieves the Current User's ID from the JWT claims, if authenticated.
        /// </summary>
        protected string? CurrentUserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        /// <summary>
        /// Retrieves the Current User's Email from the JWT claims, if authenticated.
        /// </summary>
        protected string? CurrentUserEmail => User.FindFirst(ClaimTypes.Email)?.Value;

    }
}
