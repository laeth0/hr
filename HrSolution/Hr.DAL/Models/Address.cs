using System;

namespace Hr.DAL.Models
{
    public class Address : BaseEntity
    {
        public required string Street { get; set; }
        public required string City { get; set; }
        public string? Country { get; set; }
        public string? PostalCode { get; set; }

        // Explicit FK — cleaner than the shared-PK pattern (Address.Id = Employee.Id)
        public Guid EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;
    }
}
