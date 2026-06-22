using Hr.BLL.Common;
using Hr.BLL.DTOs.Employees;
using Hr.DAL.Interfaces.MarkerInterfaces;

namespace Hr.BLL.Interfaces
{
    public interface IEmployeeService : IScopedService
    {
        Task<IEnumerable<EmployeeDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Result<EmployeeDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Result<EmployeeDto>> CreateAsync(CreateEmployeeDto dto, CancellationToken cancellationToken = default);
        Task<Result<EmployeeDto>> UpdateAsync(Guid id, UpdateEmployeeDto dto, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Result<IEnumerable<EmployeeDto>>> GetSubordinatesAsync(Guid managerId, CancellationToken cancellationToken = default);
    }
}
