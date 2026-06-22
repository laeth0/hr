using Hr.DAL.Data;
using Hr.DAL.Enums;
using Hr.DAL.Interfaces.RepositoriesInterfaces;
using Hr.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace Hr.DAL.Repositories
{
    public class LeaveRepository(ApplicationDbContext context)
        : GenericRepository<Leave>(context), ILeaveRepository
    {
        // async/await needed: ToListAsync → Task<List<Leave>>, interface → Task<IEnumerable<Leave>>
        public async Task<IEnumerable<Leave>> GetByEmployeeAsync(
            Guid employeeId,
            CancellationToken cancellationToken = default)
            => await _context.Leaves
                .AsNoTracking()
                .Where(l => l.EmployeeId == employeeId)
                .OrderByDescending(l => l.StartDate)
                .ToListAsync(cancellationToken);

        public async Task<IEnumerable<Leave>> GetByEmployeeAndYearAsync(
            Guid employeeId,
            int year,
            CancellationToken cancellationToken = default)
            => await _context.Leaves
                .AsNoTracking()
                .Where(l => l.EmployeeId == employeeId && l.StartDate.Year == year)
                .OrderByDescending(l => l.StartDate)
                .ToListAsync(cancellationToken);

        public async Task<IEnumerable<Leave>> GetPendingLeavesForManagerAsync(
            Guid managerId,
            CancellationToken cancellationToken = default)
            => await _context.Leaves
                .AsNoTracking()
                .Include(l => l.Employee)
                .Where(l => l.Employee.ManagerId == managerId
                         && l.Status == LeaveStatus.Pending)
                .OrderBy(l => l.StartDate)
                .ToListAsync(cancellationToken);

        // Overlap rule: existing leave overlaps when its start <= new end AND its end >= new start.
        // Cancelled/Rejected leaves are excluded — they no longer consume time.
        public Task<bool> HasOverlappingLeaveAsync(
            Guid employeeId,
            DateOnly startDate,
            DateOnly endDate,
            Guid? excludeLeaveId = null,
            CancellationToken cancellationToken = default)
            => _context.Leaves
                .AnyAsync(
                    l => l.EmployeeId == employeeId
                      && l.Status != LeaveStatus.Cancelled
                      && l.Status != LeaveStatus.Rejected
                      && l.StartDate <= endDate
                      && l.EndDate >= startDate
                      && (excludeLeaveId == null || l.Id != excludeLeaveId),
                    cancellationToken);
    }
}
