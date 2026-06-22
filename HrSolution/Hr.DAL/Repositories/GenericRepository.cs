using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Hr.DAL.Data;
using Hr.DAL.Interfaces.RepositoriesInterfaces;
using Hr.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace Hr.DAL.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        // FindAsync checks the DbContext's first-level cache before hitting the DB.
        // Tracked on purpose — callers may immediately follow with Update/Remove.
        // async/await needed: FindAsync returns ValueTask<T?>, interface expects Task<T?>.
        public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => await _dbSet.FindAsync([id], cancellationToken);

        // async/await needed: ToListAsync returns Task<List<T>>, but the interface
        // declares Task<IEnumerable<T>> — Task<T> is invariant, so direct return would
        // be a compile error. The await coerces List<T> → IEnumerable<T>.
        public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _dbSet.AsNoTracking().ToListAsync(cancellationToken);

        public async Task<IEnumerable<T>> FindAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default)
            => await _dbSet.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);

        // AnyAsync returns Task<bool> — matches the return type exactly, no state machine needed.
        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
            => _dbSet.AnyAsync(e => e.Id == id, cancellationToken);

        public Task<int> CountAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default)
            => _dbSet.AsNoTracking().CountAsync(predicate, cancellationToken);

        // async/await needed: AddAsync returns ValueTask<EntityEntry<T>>, interface expects Task.
        public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
            => await _dbSet.AddAsync(entity, cancellationToken);

        // AddRangeAsync returns Task — matches the return type exactly.
        public Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
            => _dbSet.AddRangeAsync(entities, cancellationToken);

        public void Update(T entity)
            => _dbSet.Update(entity);

        public void Remove(T entity)
            => _dbSet.Remove(entity);

        public void RemoveRange(IEnumerable<T> entities)
            => _dbSet.RemoveRange(entities);
    }
}
