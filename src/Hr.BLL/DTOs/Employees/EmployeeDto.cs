using Hr.DAL.Enums;

namespace Hr.BLL.DTOs.Employees
{
    public record EmployeeDto(
        Guid Id,
        string Name,
        string Email,
        decimal Salary,
        EmploymentStatus Status,
        int AllowedLeaveDayPerYear,
        Guid? ManagerId
    );
}
