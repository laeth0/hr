using Hr.BLL.Common;
using Hr.BLL.DTOs.Leaves;
using Hr.DAL.Enums;
using Hr.DAL.Interfaces.MarkerInterfaces;

namespace Hr.BLL.Interfaces
{
    public interface ILeaveService : IScopedService
    {
        Task<Result<IEnumerable<LeaveDto>>> GetByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default);
        Task<Result<LeaveDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Result<LeaveDto>> RequestLeaveAsync(Guid employeeId, CreateLeaveDto dto, CancellationToken cancellationToken = default);
        Task<Result<LeaveDto>> ApproveLeaveAsync(Guid leaveId, Guid managerId, CancellationToken cancellationToken = default);
        Task<Result<LeaveDto>> RejectLeaveAsync(Guid leaveId, Guid managerId, CancellationToken cancellationToken = default);
        Task<Result> CancelLeaveAsync(Guid leaveId, CancellationToken cancellationToken = default);
        Task<Result<int>> GetRemainingLeaveDaysAsync(Guid employeeId, LeaveType type, int year, CancellationToken cancellationToken = default);
    }
}
