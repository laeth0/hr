using Hr.DAL.Enums;

namespace Hr.BLL.DTOs.Leaves
{
    public record LeaveDto(
        Guid Id,
        DateOnly StartDate,
        DateOnly EndDate,
        LeaveType Type,
        LeaveStatus Status,
        string? Reason,
        Guid EmployeeId,
        Guid? ReviewedByManagerId
    );
}
