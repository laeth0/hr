using System;
using System.Collections.Generic;
using Hr.DAL.Enums;

namespace Hr.DAL.Models
{
    public class Employee : BaseEntity
    {
        public required string Name { get; set; }
        public required string Email { get; set; }

        // decimal — never int for money; int loses fractional values
        public decimal Salary { get; set; }

        public EmploymentStatus Status { get; set; } = EmploymentStatus.Active;
        public int AllowedLeaveDayPerYear { get; set; }

        public Guid? ManagerId { get; set; }
        public Employee? Manager { get; set; }
        public ICollection<Employee> Subordinates { get; set; } = [];

        public Address? Address { get; set; }
        public ICollection<Leave> Leaves { get; set; } = [];
    }
}
