using Hr.DAL.Data;
using Hr.DAL.Interfaces.RepositoriesInterfaces;
using Hr.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace Hr.DAL.Repositories
{
    public class AddressRepository(ApplicationDbContext context)
        : GenericRepository<Address>(context), IAddressRepository
    {
        public Task<Address?> GetByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default)
            => _dbSet.FirstOrDefaultAsync(a => a.EmployeeId == employeeId, cancellationToken);
    }
}
