using System;
using System.Collections.Generic;

namespace Hr.DAL.Models
{
    public class Employee
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Salary { get; set; }
        
        public Guid? ManagerId { get; set; }
        public Employee Manager { get; set; }
        public ICollection<Employee> Subordinates { get; set; } = new List<Employee>();

        public int AllowedLeaveDayPerYear { get; set; }

        public Address Address { get; set; }
        public ICollection<Leave> Leaves { get; set; } = new List<Leave>();
    }
}
