using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Hr.DAL.Models;

namespace Hr.DAL.Data.Configurations
{
    public class LeaveConfiguration : IEntityTypeConfiguration<Leave>
    {
        public void Configure(EntityTypeBuilder<Leave> builder)
        {
            // Store enums as readable strings in the DB
            builder.Property(l => l.Type)
                .HasConversion<string>();

            builder.Property(l => l.Status)
                .HasConversion<string>();

            builder.Property(l => l.Reason)
                .HasMaxLength(1000);

            // A leave belongs to exactly one employee
            builder
                .HasOne(l => l.Employee)
                .WithMany(e => e.Leaves)
                .HasForeignKey(l => l.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Optional: which manager approved/rejected this leave
            builder
                .HasOne(l => l.ReviewedByManager)
                .WithMany()
                .HasForeignKey(l => l.ReviewedByManagerId)
                .OnDelete(DeleteBehavior.SetNull);

            // FK columns — EF Core does not create these indexes automatically
            builder.HasIndex(l => l.EmployeeId);
            builder.HasIndex(l => l.ReviewedByManagerId);

            // Common filter: "show all pending leaves"
            builder.HasIndex(l => l.Status);

            // Composite index: used in overlap checks and "leaves by employee in year" queries
            builder.HasIndex(l => new { l.EmployeeId, l.StartDate, l.EndDate });
        }
    }
}
