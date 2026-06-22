using Hr.BLL.Common;

namespace Hr.BLL.Errors
{
    public static class EmployeeErrors
    {
        public static Error NotFound(Guid id) =>
            new("Employee.NotFound", $"Employee '{id}' was not found.", ErrorType.NotFound);

        public static readonly Error EmailConflict =
            new("Employee.EmailConflict", "An employee with this email already exists.", ErrorType.Conflict);

        public static Error ManagerNotFound(Guid id) =>
            new("Employee.ManagerNotFound", $"Manager '{id}' was not found.", ErrorType.NotFound);

        public static readonly Error SelfManagement =
            new("Employee.SelfManagement", "An employee cannot be their own manager.");

        public static readonly Error HasSubordinates =
            new("Employee.HasSubordinates", "Cannot delete an employee who still has direct reports. Reassign them first.");
    }
}
