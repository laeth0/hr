using Hr.DAL.Models;

namespace Hr.DAL.Interfaces.RepositoriesInterfaces
{
    public interface IAddressRepository : IGenericRepository<Address>
    {
        Task<Address?> GetByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default);
    }
}
