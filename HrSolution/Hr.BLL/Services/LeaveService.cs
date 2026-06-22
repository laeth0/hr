using FluentValidation;
using Hr.BLL.DTOs.Leaves;
using Hr.BLL.Exceptions;
using Hr.BLL.Interfaces;
using Hr.DAL.Enums;
using Hr.DAL.Interfaces.RepositoriesInterfaces;
using Hr.DAL.Models;
using MapsterMapper;

namespace Hr.BLL.Services
{
    public class LeaveService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidator<CreateLeaveDto> createValidator)
        : ILeaveService
    {
        public async Task<IEnumerable<LeaveDto>> GetByEmployeeAsync(
            Guid employeeId, CancellationToken cancellationToken = default)
        {
            var exists = await unitOfWork.Employees.ExistsAsync(employeeId, cancellationToken);
            if (!exists)
                throw new NotFoundException(nameof(Employee), employeeId);

            var leaves = await unitOfWork.Leaves.GetByEmployeeAsync(employeeId, cancellationToken);
            return mapper.Map<IEnumerable<LeaveDto>>(leaves);
        }

        public async Task<LeaveDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var leave = await unitOfWork.Leaves.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException(nameof(Leave), id);

            return mapper.Map<LeaveDto>(leave);
        }

        public async Task<LeaveDto> RequestLeaveAsync(
            Guid employeeId, CreateLeaveDto dto, CancellationToken cancellationToken = default)
        {
            await createValidator.ValidateAndThrowAsync(dto, cancellationToken);

            var employee = await unitOfWork.Employees.GetByIdAsync(employeeId, cancellationToken)
                ?? throw new NotFoundException(nameof(Employee), employeeId);

            var hasOverlap = await unitOfWork.Leaves.HasOverlappingLeaveAsync(
                employeeId, dto.StartDate, dto.EndDate, cancellationToken: cancellationToken);

            if (hasOverlap)
                throw new BusinessRuleException("The requested dates overlap with an existing leave.");

            if (dto.Type == LeaveType.Annual)
            {
                var remaining = CalculateRemainingDays(
                    employee, await unitOfWork.Leaves.GetByEmployeeAndYearAsync(
                        employeeId, dto.StartDate.Year, cancellationToken));

                var requestedDays = dto.EndDate.DayNumber - dto.StartDate.DayNumber + 1;
                if (requestedDays > remaining)
                {
                    throw new BusinessRuleException(
                        $"Insufficient annual leave balance. Requested: {requestedDays} day(s), remaining: {remaining}.");
                }
            }

            var leave = mapper.Map<Leave>(dto);
            leave.Id = Guid.NewGuid();
            leave.EmployeeId = employeeId;

            await unitOfWork.Leaves.AddAsync(leave, cancellationToken);
            await unitOfWork.SaveChangesAsync();

            return mapper.Map<LeaveDto>(leave);
        }

        public async Task<LeaveDto> ApproveLeaveAsync(
            Guid leaveId, Guid managerId, CancellationToken cancellationToken = default)
        {
            var leave = await unitOfWork.Leaves.GetByIdAsync(leaveId, cancellationToken)
                ?? throw new NotFoundException(nameof(Leave), leaveId);

            if (leave.Status != LeaveStatus.Pending)
                throw new BusinessRuleException($"Only pending leaves can be approved. Current status: {leave.Status}.");

            leave.Status = LeaveStatus.Approved;
            leave.ReviewedByManagerId = managerId;

            unitOfWork.Leaves.Update(leave);
            await unitOfWork.SaveChangesAsync();

            return mapper.Map<LeaveDto>(leave);
        }

        public async Task<LeaveDto> RejectLeaveAsync(
            Guid leaveId, Guid managerId, CancellationToken cancellationToken = default)
        {
            var leave = await unitOfWork.Leaves.GetByIdAsync(leaveId, cancellationToken)
                ?? throw new NotFoundException(nameof(Leave), leaveId);

            if (leave.Status != LeaveStatus.Pending)
                throw new BusinessRuleException($"Only pending leaves can be rejected. Current status: {leave.Status}.");

            leave.Status = LeaveStatus.Rejected;
            leave.ReviewedByManagerId = managerId;

            unitOfWork.Leaves.Update(leave);
            await unitOfWork.SaveChangesAsync();

            return mapper.Map<LeaveDto>(leave);
        }

        public async Task CancelLeaveAsync(Guid leaveId, CancellationToken cancellationToken = default)
        {
            var leave = await unitOfWork.Leaves.GetByIdAsync(leaveId, cancellationToken)
                ?? throw new NotFoundException(nameof(Leave), leaveId);

            if (leave.Status is LeaveStatus.Rejected or LeaveStatus.Cancelled)
                throw new BusinessRuleException($"A {leave.Status.ToString().ToLower()} leave cannot be cancelled.");

            leave.Status = LeaveStatus.Cancelled;

            unitOfWork.Leaves.Update(leave);
            await unitOfWork.SaveChangesAsync();
        }

        public async Task<int> GetRemainingLeaveDaysAsync(
            Guid employeeId, LeaveType type, int year, CancellationToken cancellationToken = default)
        {
            var employee = await unitOfWork.Employees.GetByIdAsync(employeeId, cancellationToken)
                ?? throw new NotFoundException(nameof(Employee), employeeId);

            var leaves = await unitOfWork.Leaves.GetByEmployeeAndYearAsync(employeeId, year, cancellationToken);
            return CalculateRemainingDays(employee, leaves, type);
        }

        // Calculates remaining days for a specific leave type (defaults to Annual).
        // Extracted to avoid re-fetching the employee and year leaves in RequestLeaveAsync.
        private static int CalculateRemainingDays(
            Employee employee,
            IEnumerable<Leave> yearLeaves,
            LeaveType type = LeaveType.Annual)
        {
            var usedDays = yearLeaves
                .Where(l => l.Type == type && l.Status == LeaveStatus.Approved)
                .Sum(l => l.EndDate.DayNumber - l.StartDate.DayNumber + 1);

            return employee.AllowedLeaveDayPerYear - usedDays;
        }
    }
}
