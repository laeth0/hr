using Hr.DAL.Enums;
using Hr.DAL.Models;

namespace Hr.Tests.Unit.Common;

/// <summary>
/// Lightweight test-data builders that return pre-populated domain objects.
/// Every property has a sensible default; callers override only what matters.
/// </summary>
internal static class Builders
{
    internal static Employee Employee(
        Guid? id = null,
        string name = "Alice Smith",
        string email = "alice@hr.com",
        decimal salary = 5_000m,
        EmploymentStatus status = EmploymentStatus.Active,
        int allowedLeaveDays = 20,
        Guid? managerId = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        Name = name,
        Email = email,
        Salary = salary,
        Status = status,
        AllowedLeaveDayPerYear = allowedLeaveDays,
        ManagerId = managerId,
        CreatedAt = DateTimeOffset.UtcNow,
        UpdatedAt = DateTimeOffset.UtcNow
    };

    internal static Leave Leave(
        Guid? id = null,
        Guid? employeeId = null,
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        LeaveType type = LeaveType.Annual,
        LeaveStatus status = LeaveStatus.Pending,
        string? reason = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        EmployeeId = employeeId ?? Guid.NewGuid(),
        StartDate = startDate ?? DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(1)),
        EndDate = endDate ?? DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(3)),
        Type = type,
        Status = status,
        Reason = reason,
        CreatedAt = DateTimeOffset.UtcNow,
        UpdatedAt = DateTimeOffset.UtcNow
    };

    internal static Address Address(
        Guid? id = null,
        Guid? employeeId = null,
        string street = "123 Main St",
        string city = "Springfield") => new()
    {
        Id = id ?? Guid.NewGuid(),
        EmployeeId = employeeId ?? Guid.NewGuid(),
        Street = street,
        City = city,
        CreatedAt = DateTimeOffset.UtcNow,
        UpdatedAt = DateTimeOffset.UtcNow
    };
}
