using System;

namespace Hr.DAL.Models
{
    public class Address
    {
        public Guid Id { get; set; }
        public string Street { get; set; }
        public string City { get; set; }

        public Employee Employee { get; set; }
    }
}
