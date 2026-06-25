using Hr.BLL.DTOs.Leaves;
using Hr.BLL.Interfaces;
using Hr.DAL.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Hr.PL.Controllers
{
    [Route("api/employees/{employeeId:guid}/leaves")]
    public class EmployeeLeavesController(ILeaveService leaveService) : BaseController
    {
        [HttpGet]
        public async Task<IActionResult> GetByEmployee(Guid employeeId, CancellationToken cancellationToken)
        {
            var result = await leaveService.GetByEmployeeAsync(employeeId, cancellationToken);
            return OkOrProblem(result);
        }

        [HttpPost]
        public async Task<IActionResult> RequestLeave(
            Guid employeeId, CreateLeaveDto dto, CancellationToken cancellationToken)
        {
            var result = await leaveService.RequestLeaveAsync(employeeId, dto, cancellationToken);
            return result.IsSuccess
                ? CreatedAtAction(nameof(LeavesController.GetById), "Leaves",
                    new { id = result.Value.Id }, result.Value)
                : MapError(result.Error);
        }

        [HttpGet("remaining-days")]
        public async Task<IActionResult> GetRemainingDays(
            Guid employeeId,
            [FromQuery] LeaveType type = LeaveType.Annual,
            [FromQuery] int? year = null,
            CancellationToken cancellationToken = default)
        {
            var targetYear = year ?? DateTime.UtcNow.Year;
            var result = await leaveService.GetRemainingLeaveDaysAsync(employeeId, type, targetYear, cancellationToken);
            return OkOrProblem(result);
        }
    }
}
