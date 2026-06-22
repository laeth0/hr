using Hr.BLL.Common;
using Hr.BLL.DTOs.Leaves;
using Hr.BLL.Errors;
using Hr.BLL.Interfaces;
using Hr.DAL.Enums;
using Hr.DAL.Interfaces.RepositoriesInterfaces;
using Hr.DAL.Models;
using MapsterMapper;

namespace Hr.BLL.Services
{
    public class LeaveService(IUnitOfWork unitOfWork, IMapper mapper) : ILeaveService
    {
        public async Task<Result<IEnumerable<LeaveDto>>> GetByEmployeeAsync(
            Guid employeeId, CancellationToken cancellationToken = default)
        {
            var exists = await unitOfWork.Employees.ExistsAsync(employeeId, cancellationToken);
            if (!exists)
                return Result.Failure<IEnumerable<LeaveDto>>(EmployeeErrors.NotFound(employeeId));

            var leaves = await unitOfWork.Leaves.GetByEmployeeAsync(employeeId, cancellationToken);
            return Result.Success(mapper.Map<IEnumerable<LeaveDto>>(leaves));
        }

        public async Task<Result<LeaveDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var leave = await unitOfWork.Leaves.GetByIdAsync(id, cancellationToken);

            return leave is null
                ? Result.Failure<LeaveDto>(LeaveErrors.NotFound(id))
                : Result.Success(mapper.Map<LeaveDto>(leave));
        }

        public async Task<Result<LeaveDto>> RequestLeaveAsync(
            Guid employeeId, CreateLeaveDto dto, CancellationToken cancellationToken = default)
        {
            var employee = await unitOfWork.Employees.GetByIdAsync(employeeId, cancellationToken);
            if (employee is null)
                return Result.Failure<LeaveDto>(EmployeeErrors.NotFound(employeeId));

            var hasOverlap = await unitOfWork.Leaves.HasOverlappingLeaveAsync(
                employeeId, dto.StartDate, dto.EndDate, cancellationToken: cancellationToken);

            if (hasOverlap)
                return Result.Failure<LeaveDto>(LeaveErrors.DateOverlap);

            if (dto.Type == LeaveType.Annual)
            {
                var yearLeaves = await unitOfWork.Leaves
                    .GetByEmployeeAndYearAsync(employeeId, dto.StartDate.Year, cancellationToken);

                var remaining = CalculateRemainingDays(employee, yearLeaves);
                var requestedDays = dto.EndDate.DayNumber - dto.StartDate.DayNumber + 1;

                if (requestedDays > remaining)
                    return Result.Failure<LeaveDto>(LeaveErrors.InsufficientBalance(requestedDays, remaining));
            }

            var leave = mapper.Map<Leave>(dto);
            leave.Id = Guid.NewGuid();
            leave.EmployeeId = employeeId;

            await unitOfWork.Leaves.AddAsync(leave, cancellationToken);
            await unitOfWork.SaveChangesAsync();

            return Result.Success(mapper.Map<LeaveDto>(leave));
        }

        public async Task<Result<LeaveDto>> ApproveLeaveAsync(
            Guid leaveId, Guid managerId, CancellationToken cancellationToken = default)
        {
            var leave = await unitOfWork.Leaves.GetByIdAsync(leaveId, cancellationToken);
            if (leave is null)
                return Result.Failure<LeaveDto>(LeaveErrors.NotFound(leaveId));

            if (leave.Status != LeaveStatus.Pending)
                return Result.Failure<LeaveDto>(LeaveErrors.NotPending(leave.Status));

            leave.Status = LeaveStatus.Approved;
            leave.ReviewedByManagerId = managerId;

            unitOfWork.Leaves.Update(leave);
            await unitOfWork.SaveChangesAsync();

            return Result.Success(mapper.Map<LeaveDto>(leave));
        }

        public async Task<Result<LeaveDto>> RejectLeaveAsync(
            Guid leaveId, Guid managerId, CancellationToken cancellationToken = default)
        {
            var leave = await unitOfWork.Leaves.GetByIdAsync(leaveId, cancellationToken);
            if (leave is null)
                return Result.Failure<LeaveDto>(LeaveErrors.NotFound(leaveId));

            if (leave.Status != LeaveStatus.Pending)
                return Result.Failure<LeaveDto>(LeaveErrors.NotPending(leave.Status));

            leave.Status = LeaveStatus.Rejected;
            leave.ReviewedByManagerId = managerId;

            unitOfWork.Leaves.Update(leave);
            await unitOfWork.SaveChangesAsync();

            return Result.Success(mapper.Map<LeaveDto>(leave));
        }

        public async Task<Result> CancelLeaveAsync(Guid leaveId, CancellationToken cancellationToken = default)
        {
            var leave = await unitOfWork.Leaves.GetByIdAsync(leaveId, cancellationToken);
            if (leave is null)
                return Result.Failure(LeaveErrors.NotFound(leaveId));

            if (leave.Status is LeaveStatus.Rejected or LeaveStatus.Cancelled)
                return Result.Failure(LeaveErrors.CannotCancel(leave.Status));

            leave.Status = LeaveStatus.Cancelled;

            unitOfWork.Leaves.Update(leave);
            await unitOfWork.SaveChangesAsync();

            return Result.Success();
        }

        public async Task<Result<int>> GetRemainingLeaveDaysAsync(
            Guid employeeId, LeaveType type, int year, CancellationToken cancellationToken = default)
        {
            var employee = await unitOfWork.Employees.GetByIdAsync(employeeId, cancellationToken);
            if (employee is null)
                return Result.Failure<int>(EmployeeErrors.NotFound(employeeId));

            var yearLeaves = await unitOfWork.Leaves
                .GetByEmployeeAndYearAsync(employeeId, year, cancellationToken);

            return Result.Success(CalculateRemainingDays(employee, yearLeaves, type));
        }

        // Reused by RequestLeaveAsync (quota check) and GetRemainingLeaveDaysAsync.
        // Static so it cannot accidentally close over instance state.
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
