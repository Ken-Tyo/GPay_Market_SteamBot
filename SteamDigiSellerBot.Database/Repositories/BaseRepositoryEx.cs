using DatabaseRepository.Repositories;
using Microsoft.EntityFrameworkCore;
using SteamDigiSellerBot.Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DatabaseRepository.Entities;

namespace SteamDigiSellerBot.Database.Repositories
{
    public interface IBaseRepositoryEx<T>: IBaseRepository<T> where T : BaseEntity
    {
        Task<bool> UpdateFieldAsync<TField>(T item, Expression<Func<T, TField>> predicate);
        Task<bool> UpdateFieldAsync<TField>(DbContext db, T item, Expression<Func<T, TField>> predicate);
        Task<bool> UpdateFieldsAsync(T item, params Expression<Func<T, object>>[] predicate);
        Task<bool> UpdateFieldsAsync(DbContext db, T item, params Expression<Func<T, object>>[] predicate);
    }

    public class BaseRepositoryEx<T> : BaseRepository<T> where T : BaseEntity
    {
        private readonly IDbContextFactory<DbContext> factory;

        public BaseRepositoryEx(IDbContextFactory<DbContext> databaseContext) : base(databaseContext)
        {
            this.factory = databaseContext;
        }

        public async Task<bool> UpdateFieldAsync<TField>(T item, Expression<Func<T, TField>> predicate)
        {
            await using var db = factory.CreateDbContext();
            return await UpdateFieldAsync(db, item, predicate);
        }
        public async Task<bool> UpdateFieldAsync<TField>(DbContext db, T item, Expression<Func<T, TField>> predicate)
        {
            db.Attach(item);
            db.Entry(item).Property(predicate).IsModified = true;
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateFieldsAsync(T item, params Expression<Func<T, object>>[] predicate)
        {
            await using var db = factory.CreateDbContext();
            return await UpdateFieldsAsync(db, item, predicate);
        }

        public async Task<bool> UpdateFieldsAsync(DbContext db, T item, params Expression<Func<T, object>>[] predicate)
        {
            db.Attach(item);
            foreach (var  p in predicate)
            {
                db.Entry(item).Property(p).IsModified = true;
            }
            await db.SaveChangesAsync();
            return true;
        }
    }


}
