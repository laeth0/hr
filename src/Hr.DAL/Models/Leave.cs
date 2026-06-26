using System;
using Hr.DAL.Enums;

namespace Hr.DAL.Models
{
    public class Leave : BaseEntity
    {
        // DateOnly — leaves are whole calendar days, not timestamps
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

        public LeaveType Type { get; set; }
        public LeaveStatus Status { get; set; } = LeaveStatus.Pending;

        public string? Reason { get; set; }

        public Guid EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;

        public Guid? ReviewedByManagerId { get; set; }
        public Employee? ReviewedByManager { get; set; }
    }
}
