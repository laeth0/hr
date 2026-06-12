using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using Hr.DAL.Interfaces;
using Hr.DAL.Repositories.Interfaces;

namespace Hr.DAL.UnitOfWork
{
    public interface IUnitOfWork : IDisposable, IScopedService
    {
        IEmployeeRepository Employees { get; }
        ILeaveRepository Leaves { get; }
        IAddressRepository Addresses { get; }

        Task<int> SaveChangesAsync();

        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitAsync(CancellationToken cancellationToken = default);
        Task RollbackAsync(CancellationToken cancellationToken = default);
        IExecutionStrategy CreateExecutionStrategy();
    }
}
