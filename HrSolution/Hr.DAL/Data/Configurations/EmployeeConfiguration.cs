using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Hr.DAL.Models;

namespace Hr.DAL.Data.Configurations
{
    public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
    {
        public void Configure(EntityTypeBuilder<Employee> builder)
        {
            builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(256);

            // Unique email — no two employees share the same address
            builder.HasIndex(e => e.Email)
                .IsUnique();

            // numeric(18,2) maps to PostgreSQL NUMERIC — never FLOAT for money
            builder.Property(e => e.Salary)
                .HasColumnType("numeric(18,2)");

            // Store enum as readable string in the DB instead of an opaque integer
            builder.Property(e => e.Status)
                .HasConversion<string>();

            // Self-referencing manager hierarchy
            builder
                .HasOne(e => e.Manager)
                .WithMany(e => e.Subordinates)
                .HasForeignKey(e => e.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            // FK columns are NOT auto-indexed by EF Core — must be explicit
            builder.HasIndex(e => e.ManagerId);

            // Common filter: "show only active employees"
            builder.HasIndex(e => e.Status);

        }
    }
}
