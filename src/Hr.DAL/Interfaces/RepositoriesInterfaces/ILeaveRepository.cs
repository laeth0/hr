using Hr.DAL.Models;

namespace Hr.DAL.Interfaces.RepositoriesInterfaces
{
    public interface ILeaveRepository : IGenericRepository<Leave>
    {
        /// <summary>All leaves for a specific employee, newest first.</summary>
        Task<IEnumerable<Leave>> GetByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default);

        /// <summary>Leaves for an employee within a specific calendar year — used to calculate quota usage.</summary>
        Task<IEnumerable<Leave>> GetByEmployeeAndYearAsync(Guid employeeId, int year, CancellationToken cancellationToken = default);

        /// <summary>All pending leave requests for subordinates of a given manager.</summary>
        Task<IEnumerable<Leave>> GetPendingLeavesForManagerAsync(Guid managerId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns true if the employee already has a non-cancelled/non-rejected leave that
        /// overlaps the proposed date range. Pass excludeLeaveId when editing an existing leave.
        /// </summary>
        Task<bool> HasOverlappingLeaveAsync(
            Guid employeeId,
            DateOnly startDate,
            DateOnly endDate,
            Guid? excludeLeaveId = null,
            CancellationToken cancellationToken = default);
    }
}
