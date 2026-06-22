using Hr.BLL.DTOs.Leaves;
using Hr.DAL.Enums;
using Hr.DAL.Interfaces.MarkerInterfaces;

namespace Hr.BLL.Interfaces
{
    public interface ILeaveService : IScopedService
    {
        Task<IEnumerable<LeaveDto>> GetByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default);
        Task<LeaveDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<LeaveDto> RequestLeaveAsync(Guid employeeId, CreateLeaveDto dto, CancellationToken cancellationToken = default);
        Task<LeaveDto> ApproveLeaveAsync(Guid leaveId, Guid managerId, CancellationToken cancellationToken = default);
        Task<LeaveDto> RejectLeaveAsync(Guid leaveId, Guid managerId, CancellationToken cancellationToken = default);
        Task CancelLeaveAsync(Guid leaveId, CancellationToken cancellationToken = default);
        Task<int> GetRemainingLeaveDaysAsync(Guid employeeId, LeaveType type, int year, CancellationToken cancellationToken = default);
    }
}
