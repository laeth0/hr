using Hr.DAL.Enums;

namespace Hr.BLL.DTOs.Employees
{
    public record UpdateEmployeeDto(
        string Name,
        decimal Salary,
        EmploymentStatus Status,
        int AllowedLeaveDayPerYear,
        Guid? ManagerId
    );
}
