using System;
using System.Threading.Tasks;
using Hr.DAL.Repositories.Interfaces;

namespace Hr.DAL.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IEmployeeRepository Employees { get; }
        ILeaveRepository Leaves { get; }
        IAddressRepository Addresses { get; }

        Task<int> SaveChangesAsync();
    }
}
