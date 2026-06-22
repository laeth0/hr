using Hr.DAL.Enums;

namespace Hr.BLL.DTOs.Leaves
{
    public record CreateLeaveDto(
        DateOnly StartDate,
        DateOnly EndDate,
        LeaveType Type,
        string? Reason
    );
}
