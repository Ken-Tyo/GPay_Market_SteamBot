using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using DatabaseRepository.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace DatabaseRepository.Repositories
{
#warning Старая библиотека Sanyocheck.DatabaseRepository работала с одиним контекстом базы данных выстраивая больших очереди для единого контекста
    public class BaseRepository<T> : IBaseRepository<T> where T : BaseEntity
    {
        private readonly IDbContextFactory<DbContext> _databaseContextFactory;

        public BaseRepository(IDbContextFactory<DbContext> databaseContextFactory) =>
            this._databaseContextFactory = databaseContextFactory;

        public async Task<T> GetByIdAsync(int id)
        {
            await using var databaseContext = _databaseContextFactory.CreateDbContext();
            return await GetByIdAsync(databaseContext, id);
        }
        public async Task<T> GetByIdAsync(DbContext databaseContext, int id)
        {
            return await databaseContext.Set<T>().FindAsync((object)id);
        }

        public virtual async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            await using var databaseContext = _databaseContextFactory.CreateDbContext();
            return await CountAsync(databaseContext, cancellationToken);
        }
        public virtual async Task<int> CountAsync(DbContext databaseContext, CancellationToken cancellationToken = default)
        {
            return await databaseContext.Set<T>().CountAsync<T>();
        }


        public async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            await using var databaseContext = _databaseContextFactory.CreateDbContext();
            return await CountAsync(databaseContext, predicate);
        }
        public async Task<int> CountAsync(DbContext databaseContext, Expression<Func<T, bool>> predicate)
        {
            return await databaseContext.Set<T>().CountAsync<T>(predicate);
        }

        public async Task<bool> ExistByPredicateAsync(Expression<Func<T, bool>> predicate)
        {
            await using var databaseContext = _databaseContextFactory.CreateDbContext();
            return await ExistByPredicateAsync(databaseContext, predicate);
        }
        public async Task<bool> ExistByPredicateAsync(DbContext databaseContext, Expression<Func<T, bool>> predicate)
        {
            return await databaseContext.Set<T>().AnyAsync<T>(predicate);
        }

        public async Task<T> GetFirstAsync()
        {
            await using var databaseContext = _databaseContextFactory.CreateDbContext();
            return await GetFirstAsync(databaseContext);
        }
        public async Task<T> GetFirstAsync(DbContext databaseContext)
        {
            return await databaseContext.Set<T>().FirstOrDefaultAsync<T>();
        }

        public async Task<T> GetByPredicateAsync(Expression<Func<T, bool>> predicate)
        {
            await using var databaseContext = _databaseContextFactory.CreateDbContext();
            return await GetByPredicateAsync(databaseContext, predicate);
        }
        public async Task<T> GetByPredicateAsync(DbContext databaseContext, Expression<Func<T, bool>> predicate)
        {
            return await databaseContext.Set<T>().FirstOrDefaultAsync<T>(predicate);
        }

        public async Task<List<T>> ListAsync(CancellationToken cancellationToken = default)
        {
            await using var databaseContext = _databaseContextFactory.CreateDbContext();
            return await ListAsync(databaseContext, cancellationToken);
        }
        public async Task<List<T>> ListAsync(DbContext databaseContext, CancellationToken cancellationToken = default)
        {
            return await databaseContext.Set<T>().ToListAsync(cancellationToken);
        }

        public async Task<List<T>> ListAsync(Expression<Func<T, bool>> predicate)
        {
            await using var databaseContext = _databaseContextFactory.CreateDbContext();
            return await ListAsync(databaseContext, predicate);
        }
        public async Task<List<T>> ListAsync(DbContext databaseContext, Expression<Func<T, bool>> predicate)
        {
            return await databaseContext.Set<T>().Where<T>(predicate).ToListAsync<T>();
        }

        public async Task<List<T>> ListAsync(int start, int end)
        {
            await using var databaseContext = _databaseContextFactory.CreateDbContext();
            return await ListAsync(databaseContext, start,end);
        }
        public async Task<List<T>> ListAsync(DbContext databaseContext, int start, int end)
        {
            return await databaseContext.Set<T>().Skip<T>(start).Take<T>(end).ToListAsync<T>();
        }

        public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await using var databaseContext = _databaseContextFactory.CreateDbContext();
            await AddAsync(databaseContext,entity, cancellationToken);
        }
        public async Task AddAsync(DbContext databaseContext, T entity, CancellationToken cancellationToken = default)
        {
            EntityEntry<T> entityEntry = await databaseContext.Set<T>().AddAsync(entity, cancellationToken);
            int num = await databaseContext.SaveChangesAsync(cancellationToken);
        }



        public async Task AddRangeAsync(IEnumerable<T> entity)
        {
            await using var databaseContext = _databaseContextFactory.CreateDbContext();
            await AddRangeAsync(databaseContext, entity);
        }
        public async Task AddRangeAsync(DbContext databaseContext, IEnumerable<T> entity)
        {
            await databaseContext.Set<T>().AddRangeAsync(entity);
            int num = await databaseContext.SaveChangesAsync();
        }



        public async Task EditAsync(T entity)
        {
            await using var databaseContext = _databaseContextFactory.CreateDbContext();
            await EditAsync(databaseContext, entity);
        }
        public async Task EditAsync(DbContext databaseContext, T entity)
        {
            databaseContext.Entry<T>(entity).State = EntityState.Modified;
            int num = await databaseContext.SaveChangesAsync();
        }

        public async Task EditAsync(IEnumerable<T> entities)
        {
            await using var databaseContext = _databaseContextFactory.CreateDbContext();
            await EditAsync(databaseContext, entities);
        }
        public async Task EditAsync(DbContext databaseContext, IEnumerable<T> entities)
        {
            foreach (T entity in entities)
                databaseContext.Entry<T>(entity).State = EntityState.Modified;
            int num = await databaseContext.SaveChangesAsync();
        }

        public async Task ReplaceAsync(T oldEntity, T newEntity)
        {
            await using var databaseContext = _databaseContextFactory.CreateDbContext();
            await ReplaceAsync(databaseContext, oldEntity, newEntity);
        }
        public async Task ReplaceAsync(DbContext databaseContext, T oldEntity, T newEntity)
        {
            newEntity.Id = oldEntity.Id;
            databaseContext.Entry<T>(oldEntity).CurrentValues.SetValues((object)newEntity);
            int num = await databaseContext.SaveChangesAsync();
        }

        public DbContext GetContext()
        {
            return _databaseContextFactory.CreateDbContext();
        }

        public virtual async Task<int> DeleteAsync(T entity, CancellationToken cancellationToken = default)
        {
            await using var databaseContext = _databaseContextFactory.CreateDbContext();
            return await DeleteAsync(databaseContext, entity, cancellationToken);
        }
        public virtual async Task<int> DeleteAsync(DbContext databaseContext, T entity, CancellationToken cancellationToken = default)
        {
            databaseContext.Set<T>().Remove(entity);
            return await databaseContext.SaveChangesAsync();
        }

        public virtual async Task DeleteListAsync(IEnumerable<T> entities)
        {
            await using var databaseContext = _databaseContextFactory.CreateDbContext();
            await DeleteListAsync(databaseContext, entities);
        }
        public virtual async Task DeleteListAsync(DbContext databaseContext, IEnumerable<T> entities)
        {
            databaseContext.Set<T>().RemoveRange(entities);
            int num = await databaseContext.SaveChangesAsync();
        }

        public async Task<bool> AnyAsync()
        {
            await using var databaseContext = _databaseContextFactory.CreateDbContext();
            return await AnyAsync(databaseContext);
        }
        public async Task<bool> AnyAsync(DbContext databaseContext)
        {
            return await databaseContext.Set<T>().AnyAsync<T>();
        }

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            await using var databaseContext = _databaseContextFactory.CreateDbContext();
            return await AnyAsync(databaseContext, predicate);
        }
        public async Task<bool> AnyAsync(DbContext databaseContext, Expression<Func<T, bool>> predicate)
        {
            return await databaseContext.Set<T>().AnyAsync<T>(predicate);
        }

        public async Task<List<T>> OrderByAsync<TKey>(Expression<Func<T, TKey>> predicate)
        {
            await using var databaseContext = _databaseContextFactory.CreateDbContext();
            return await OrderByAsync(databaseContext,predicate);
        }
        public async Task<List<T>> OrderByAsync<TKey>(DbContext databaseContext, Expression<Func<T, TKey>> predicate)
        {
            return await databaseContext.Set<T>().OrderBy<T, TKey>(predicate).ToListAsync<T>();
        }


        public async Task<List<T>> OrderByDescendingAsync<TKey>(Expression<Func<T, TKey>> predicate)
        {
            await using var databaseContext = _databaseContextFactory.CreateDbContext();
            return await OrderByDescendingAsync(databaseContext, predicate);
        }
        public async Task<List<T>> OrderByDescendingAsync<TKey>(DbContext databaseContext, Expression<Func<T, TKey>> predicate)
        {
            return await databaseContext.Set<T>().OrderByDescending<T, TKey>(predicate).ToListAsync<T>();
        }
    }

    public interface IBaseRepository<T> where T : class
    {
        Task<int> CountAsync(CancellationToken cancellationToken = default);
        Task<int> CountAsync(DbContext databaseContext, CancellationToken cancellationToken = default);

        Task<bool> AnyAsync();
        Task<bool> AnyAsync(DbContext databaseContext);

        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
        Task<bool> AnyAsync(DbContext databaseContext, Expression<Func<T, bool>> predicate);

        Task<List<T>> OrderByDescendingAsync<TKey>(Expression<Func<T, TKey>> predicate);
        Task<List<T>> OrderByDescendingAsync<TKey>(DbContext databaseContext, Expression<Func<T, TKey>> predicate);

        Task<List<T>> OrderByAsync<TKey>(Expression<Func<T, TKey>> predicate);
        Task<List<T>> OrderByAsync<TKey>(DbContext databaseContext, Expression<Func<T, TKey>> predicate);

        Task<int> CountAsync(Expression<Func<T, bool>> predicate);
        Task<int> CountAsync(DbContext databaseContext, Expression<Func<T, bool>> predicate);

        Task<bool> ExistByPredicateAsync(Expression<Func<T, bool>> predicate);
        Task<bool> ExistByPredicateAsync(DbContext databaseContext, Expression<Func<T, bool>> predicate);

        Task<T> GetByIdAsync(int id);
        Task<T> GetByIdAsync(DbContext databaseContext, int id);

        Task<T> GetFirstAsync();
        Task<T> GetFirstAsync(DbContext databaseContext);

        Task<T> GetByPredicateAsync(Expression<Func<T, bool>> predicate);
        Task<T> GetByPredicateAsync(DbContext databaseContext, Expression<Func<T, bool>> predicate);

        Task<List<T>> ListAsync(CancellationToken cancellationToken = default);
        Task<List<T>> ListAsync(DbContext databaseContext, CancellationToken cancellationToken = default);

        Task<List<T>> ListAsync(Expression<Func<T, bool>> predicate);
        Task<List<T>> ListAsync(DbContext databaseContext, Expression<Func<T, bool>> predicate);

        Task<List<T>> ListAsync(int start, int end);
        Task<List<T>> ListAsync(DbContext databaseContext, int start, int end);

        Task AddAsync(T entity, CancellationToken cancellationToken = default);
        Task AddAsync(DbContext databaseContext, T entity, CancellationToken cancellationToken = default);

        Task AddRangeAsync(IEnumerable<T> entity);
        Task AddRangeAsync(DbContext databaseContext, IEnumerable<T> entity);

        Task<int> DeleteAsync(T entity, CancellationToken cancellationToken = default);
        Task<int> DeleteAsync(DbContext databaseContext, T entity, CancellationToken cancellationToken = default);

        Task DeleteListAsync(IEnumerable<T> entities);
        Task DeleteListAsync(DbContext databaseContext, IEnumerable<T> entities);

        Task EditAsync(T entity);
        Task EditAsync(DbContext databaseContext, T entity);

        Task EditAsync(IEnumerable<T> entities);
        Task EditAsync(DbContext databaseContext, IEnumerable<T> entities);

        Task ReplaceAsync(T oldEntity, T newEntity);
        Task ReplaceAsync(DbContext databaseContext, T oldEntity, T newEntity);

        DbContext GetContext();
    }

}

namespace DatabaseRepository.Entities
{

    public class BaseEntity
    {
        public int Id { get; set; }
    }
}