using Hr.DAL.Data;
using Hr.DAL.Interfaces.RepositoriesInterfaces;
using Hr.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace Hr.DAL.Repositories
{
    public class EmployeeRepository(ApplicationDbContext context)
        : GenericRepository<Employee>(context), IEmployeeRepository
    {
        public Task<Employee?> GetWithAddressAsync(Guid id, CancellationToken cancellationToken = default)
            => _context.Employees
                .AsNoTracking()
                .Include(e => e.Address)
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        public Task<Employee?> GetWithLeavesAsync(Guid id, CancellationToken cancellationToken = default)
            => _context.Employees
                .AsNoTracking()
                .Include(e => e.Leaves)
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        // async/await needed: ToListAsync returns Task<List<T>>, interface returns Task<IEnumerable<T>>.
        public async Task<IEnumerable<Employee>> GetSubordinatesAsync(
            Guid managerId,
            CancellationToken cancellationToken = default)
            => await _context.Employees
                .AsNoTracking()
                .Where(e => e.ManagerId == managerId)
                .ToListAsync(cancellationToken);

        public Task<Employee?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
            => _context.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Email == email, cancellationToken);
    }
}
