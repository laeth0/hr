using Hr.BLL.DTOs.Employees;
using Hr.DAL.Interfaces.MarkerInterfaces;

namespace Hr.BLL.Interfaces
{
    public interface IEmployeeService : IScopedService
    {
        Task<IEnumerable<EmployeeDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<EmployeeDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<EmployeeDto> CreateAsync(CreateEmployeeDto dto, CancellationToken cancellationToken = default);
        Task<EmployeeDto> UpdateAsync(Guid id, UpdateEmployeeDto dto, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<EmployeeDto>> GetSubordinatesAsync(Guid managerId, CancellationToken cancellationToken = default);
    }
}
