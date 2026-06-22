using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Hr.DAL.Models;

namespace Hr.DAL.Data.Configurations
{
    public class AddressConfiguration : IEntityTypeConfiguration<Address>
    {
        public void Configure(EntityTypeBuilder<Address> builder)
        {
            builder.Property(a => a.Street)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(a => a.City)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(a => a.Country)
                .HasMaxLength(100);

            builder.Property(a => a.PostalCode)
                .HasMaxLength(20);

            // Address is the dependent in the 1:1 — it carries the FK to Employee.
            // Using an explicit EmployeeId column instead of the shared-PK pattern.
            builder
                .HasOne(a => a.Employee)
                .WithOne(e => e.Address)
                .HasForeignKey<Address>(a => a.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique index on EmployeeId enforces the 1:1 at the database level
            builder.HasIndex(a => a.EmployeeId)
                .IsUnique();
        }
    }
}
