namespace Hr.BLL.DTOs.Employees
{
    public record CreateEmployeeDto(
        string Name,
        string Email,
        decimal Salary,
        int AllowedLeaveDayPerYear,
        Guid? ManagerId
    );
}
