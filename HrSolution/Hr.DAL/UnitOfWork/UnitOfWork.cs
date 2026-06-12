using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Hr.DAL.Data;
using Hr.DAL.Repositories;
using Hr.DAL.Repositories.Interfaces;

namespace Hr.DAL.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private bool _disposed;

        // Backing fields — repository is created only on first access
        private readonly Lazy<IEmployeeRepository> _employees;
        private readonly Lazy<ILeaveRepository> _leaves;
        private readonly Lazy<IAddressRepository> _addresses;

        public IEmployeeRepository Employees => _employees.Value;
        public ILeaveRepository Leaves       => _leaves.Value;
        public IAddressRepository Addresses  => _addresses.Value;

        public UnitOfWork(
            ApplicationDbContext context,
            Lazy<IEmployeeRepository> employees,
            Lazy<ILeaveRepository> leaves,
            Lazy<IAddressRepository> addresses)
        {
            _context   = context;
            _employees = employees;
            _leaves    = leaves;
            _addresses = addresses;
        }

        public async Task<int> SaveChangesAsync()
            => await _context.SaveChangesAsync();

        public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            if (_context.Database.CurrentTransaction is null) return;

            await _context.Database.CommitTransactionAsync(cancellationToken);
        }

        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            if (_context.Database.CurrentTransaction is null) return;

            await _context.Database.RollbackTransactionAsync(cancellationToken);
        }

        public IExecutionStrategy CreateExecutionStrategy()
        {
            return _context.Database.CreateExecutionStrategy();
        }

        // ── IDisposable pattern ───────────────────────────────────────────────

        /// <summary>
        /// Releases managed resources (e.g. DbContext) when called explicitly,
        /// and skips them when called from the finalizer (managed refs may already
        /// be collected at that point).
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                // Managed resources — safe to free only on explicit Dispose()
                _context.Dispose();
            }

            // Unmanaged resources (if any) would be freed here regardless of path

            _disposed = true;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(disposing: true);

            // Tell the GC not to call the finalizer — we've already cleaned up.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizer — safety net in case the caller forgets to call Dispose().
        /// Calls Dispose(false) so we only free unmanaged resources here.
        /// </summary>
        ~UnitOfWork() => Dispose(disposing: false);
    }
}
