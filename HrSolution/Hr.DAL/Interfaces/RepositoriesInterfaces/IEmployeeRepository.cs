using Hr.DAL.Models;

namespace Hr.DAL.Interfaces.RepositoriesInterfaces
{
    public interface IEmployeeRepository : IGenericRepository<Employee>
    {
        /// <summary>Loads the employee together with their address in one query.</summary>
        Task<Employee?> GetWithAddressAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>Loads the employee together with all their leave records.</summary>
        Task<Employee?> GetWithLeavesAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>Returns all direct reports of the given manager.</summary>
        Task<IEnumerable<Employee>> GetSubordinatesAsync(Guid managerId, CancellationToken cancellationToken = default);

        /// <summary>Looks up an employee by email — used during authentication and validation.</summary>
        Task<Employee?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    }
}
