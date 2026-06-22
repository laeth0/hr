using FluentValidation;
using Hr.BLL.Common;
using Hr.BLL.DTOs.Employees;
using Hr.BLL.Errors;
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

        public async Task<Result<EmployeeDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var employee = await unitOfWork.Employees.GetByIdAsync(id, cancellationToken);

            return employee is null
                ? Result.Failure<EmployeeDto>(EmployeeErrors.NotFound(id))
                : Result.Success(mapper.Map<EmployeeDto>(employee));
        }

        public async Task<Result<EmployeeDto>> CreateAsync(
            CreateEmployeeDto dto, CancellationToken cancellationToken = default)
        {
            var validation = await createValidator.ValidateAsync(dto, cancellationToken);
            if (!validation.IsValid)
            {
                return Result.Failure<EmployeeDto>(
                    Error.Validation(validation.Errors.Select(e => e.ErrorMessage)));
            }

            var emailTaken = await unitOfWork.Employees.GetByEmailAsync(dto.Email, cancellationToken);
            if (emailTaken is not null)
                return Result.Failure<EmployeeDto>(EmployeeErrors.EmailConflict);

            if (dto.ManagerId.HasValue)
            {
                var managerExists = await unitOfWork.Employees.ExistsAsync(dto.ManagerId.Value, cancellationToken);
                if (!managerExists)
                    return Result.Failure<EmployeeDto>(EmployeeErrors.ManagerNotFound(dto.ManagerId.Value));
            }

            var employee = mapper.Map<Employee>(dto);
            employee.Id = Guid.NewGuid();

            await unitOfWork.Employees.AddAsync(employee, cancellationToken);
            await unitOfWork.SaveChangesAsync();

            return Result.Success(mapper.Map<EmployeeDto>(employee));
        }

        public async Task<Result<EmployeeDto>> UpdateAsync(
            Guid id, UpdateEmployeeDto dto, CancellationToken cancellationToken = default)
        {
            var validation = await updateValidator.ValidateAsync(dto, cancellationToken);
            if (!validation.IsValid)
            {
                return Result.Failure<EmployeeDto>(
                    Error.Validation(validation.Errors.Select(e => e.ErrorMessage)));
            }

            var employee = await unitOfWork.Employees.GetByIdAsync(id, cancellationToken);
            if (employee is null)
                return Result.Failure<EmployeeDto>(EmployeeErrors.NotFound(id));

            if (dto.ManagerId == id)
                return Result.Failure<EmployeeDto>(EmployeeErrors.SelfManagement);

            if (dto.ManagerId.HasValue)
            {
                var managerExists = await unitOfWork.Employees.ExistsAsync(dto.ManagerId.Value, cancellationToken);
                if (!managerExists)
                    return Result.Failure<EmployeeDto>(EmployeeErrors.ManagerNotFound(dto.ManagerId.Value));
            }

            // Explicit field assignment — prevents accidental overwrite of Email and
            // other identity fields that are not part of a standard update.
            employee.Name = dto.Name;
            employee.Salary = dto.Salary;
            employee.Status = dto.Status;
            employee.AllowedLeaveDayPerYear = dto.AllowedLeaveDayPerYear;
            employee.ManagerId = dto.ManagerId;

            unitOfWork.Employees.Update(employee);
            await unitOfWork.SaveChangesAsync();

            return Result.Success(mapper.Map<EmployeeDto>(employee));
        }

        public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var employee = await unitOfWork.Employees.GetByIdAsync(id, cancellationToken);
            if (employee is null)
                return Result.Failure(EmployeeErrors.NotFound(id));

            var subordinates = await unitOfWork.Employees.GetSubordinatesAsync(id, cancellationToken);
            if (subordinates.Any())
                return Result.Failure(EmployeeErrors.HasSubordinates);

            unitOfWork.Employees.Remove(employee);
            await unitOfWork.SaveChangesAsync();

            return Result.Success();
        }

        public async Task<Result<IEnumerable<EmployeeDto>>> GetSubordinatesAsync(
            Guid managerId, CancellationToken cancellationToken = default)
        {
            var managerExists = await unitOfWork.Employees.ExistsAsync(managerId, cancellationToken);
            if (!managerExists)
                return Result.Failure<IEnumerable<EmployeeDto>>(EmployeeErrors.NotFound(managerId));

            var subordinates = await unitOfWork.Employees.GetSubordinatesAsync(managerId, cancellationToken);
            return Result.Success(mapper.Map<IEnumerable<EmployeeDto>>(subordinates));
        }
    }
}
