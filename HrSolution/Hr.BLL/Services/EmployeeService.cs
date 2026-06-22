using FluentValidation;
using Hr.BLL.DTOs.Employees;
using Hr.BLL.Exceptions;
using Hr.BLL.Interfaces;
using Hr.DAL.Interfaces.RepositoriesInterfaces;
using Hr.DAL.Models;
using MapsterMapper;

namespace Hr.BLL.Services
{
    public class EmployeeService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidator<CreateEmployeeDto> createValidator,
        IValidator<UpdateEmployeeDto> updateValidator)
        : IEmployeeService
    {
        public async Task<IEnumerable<EmployeeDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var employees = await unitOfWork.Employees.GetAllAsync(cancellationToken);
            return mapper.Map<IEnumerable<EmployeeDto>>(employees);
        }

        public async Task<EmployeeDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var employee = await unitOfWork.Employees.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException(nameof(Employee), id);

            return mapper.Map<EmployeeDto>(employee);
        }

        public async Task<EmployeeDto> CreateAsync(CreateEmployeeDto dto, CancellationToken cancellationToken = default)
        {
            await createValidator.ValidateAndThrowAsync(dto, cancellationToken);

            var emailTaken = await unitOfWork.Employees.GetByEmailAsync(dto.Email, cancellationToken);
            if (emailTaken is not null)
                throw new BusinessRuleException($"An employee with email '{dto.Email}' already exists.");

            if (dto.ManagerId.HasValue)
            {
                var managerExists = await unitOfWork.Employees.ExistsAsync(dto.ManagerId.Value, cancellationToken);
                if (!managerExists)
                    throw new NotFoundException(nameof(Employee), dto.ManagerId.Value);
            }

            var employee = mapper.Map<Employee>(dto);
            employee.Id = Guid.NewGuid();

            await unitOfWork.Employees.AddAsync(employee, cancellationToken);
            await unitOfWork.SaveChangesAsync();

            return mapper.Map<EmployeeDto>(employee);
        }

        public async Task<EmployeeDto> UpdateAsync(
            Guid id, UpdateEmployeeDto dto, CancellationToken cancellationToken = default)
        {
            await updateValidator.ValidateAndThrowAsync(dto, cancellationToken);

            var employee = await unitOfWork.Employees.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException(nameof(Employee), id);

            if (dto.ManagerId == id)
                throw new BusinessRuleException("An employee cannot be their own manager.");

            if (dto.ManagerId.HasValue)
            {
                var managerExists = await unitOfWork.Employees.ExistsAsync(dto.ManagerId.Value, cancellationToken);
                if (!managerExists)
                    throw new NotFoundException(nameof(Employee), dto.ManagerId.Value);
            }

            // Explicit assignment — prevents accidental overwrite of Email and other
            // identity fields that are not part of a standard update operation.
            employee.Name = dto.Name;
            employee.Salary = dto.Salary;
            employee.Status = dto.Status;
            employee.AllowedLeaveDayPerYear = dto.AllowedLeaveDayPerYear;
            employee.ManagerId = dto.ManagerId;

            unitOfWork.Employees.Update(employee);
            await unitOfWork.SaveChangesAsync();

            return mapper.Map<EmployeeDto>(employee);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var employee = await unitOfWork.Employees.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException(nameof(Employee), id);

            var subordinates = await unitOfWork.Employees.GetSubordinatesAsync(id, cancellationToken);
            if (subordinates.Any())
            {
                throw new BusinessRuleException(
                    "Cannot delete an employee who still has subordinates. Reassign them first.");
            }

            unitOfWork.Employees.Remove(employee);
            await unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<EmployeeDto>> GetSubordinatesAsync(
            Guid managerId, CancellationToken cancellationToken = default)
        {
            var managerExists = await unitOfWork.Employees.ExistsAsync(managerId, cancellationToken);
            if (!managerExists)
                throw new NotFoundException(nameof(Employee), managerId);

            var subordinates = await unitOfWork.Employees.GetSubordinatesAsync(managerId, cancellationToken);
            return mapper.Map<IEnumerable<EmployeeDto>>(subordinates);
        }
    }
}
