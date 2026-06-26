using Hr.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Hr.PL.Controllers
{
    [Route("api/leaves")]
    public class LeavesController(ILeaveService leaveService) : BaseController
    {
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await leaveService.GetByIdAsync(id, cancellationToken);
            return OkOrProblem(result);
        }

        [HttpPut("{id:guid}/approve")]
        public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(CurrentUserId, out var managerId))
                return Unauthorized();

            var result = await leaveService.ApproveLeaveAsync(id, managerId, cancellationToken);
            return OkOrProblem(result);
        }

        [HttpPut("{id:guid}/reject")]
        public async Task<IActionResult> Reject(Guid id, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(CurrentUserId, out var managerId))
                return Unauthorized();

            var result = await leaveService.RejectLeaveAsync(id, managerId, cancellationToken);
            return OkOrProblem(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
        {
            var result = await leaveService.CancelLeaveAsync(id, cancellationToken);
            return NoContentOrProblem(result);
        }
    }
}
