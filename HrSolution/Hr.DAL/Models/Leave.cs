using System;
using Hr.DAL.Enums;

namespace Hr.DAL.Models
{
    public class Leave
    {
        public Guid Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public LeaveType Type { get; set; }
        
        public Guid EmployeeId { get; set; }
        public Employee Employee { get; set; }
    }
}
