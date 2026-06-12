using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Hr.DAL.Models;

namespace Hr.DAL.Data.Configurations
{
    public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
    {
        public void Configure(EntityTypeBuilder<Employee> builder)
        {
            // Configure self-referencing relationship for Employee
            builder
                .HasOne(e => e.Manager)
                .WithMany(e => e.Subordinates)
                .HasForeignKey(e => e.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure 1-to-1 relationship between Employee and Address
            builder
                .HasOne(e => e.Address)
                .WithOne(a => a.Employee)
                .HasForeignKey<Address>(a => a.Id); // Address.Id is the foreign key
        }
    }
}
