using DatabaseRepository.Entities;
using DatabaseRepository.Repositories;
using Microsoft.EntityFrameworkCore;
using SteamDigiSellerBot.Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Database.Repositories
{
    public interface IBaseRepositoryEx<T>: IBaseRepository<T> where T : BaseEntity
    {
        Task<bool> UpdateFieldAsync<TField>(T item, Expression<Func<T, TField>> predicate);
    }

    public class BaseRepositoryEx<T> : BaseRepository<T> where T : BaseEntity
    {
        private readonly DbContext db;

        public BaseRepositoryEx(DbContext databaseContext) : base(databaseContext)
        {
            this.db = databaseContext;
        }

        public async Task<bool> UpdateFieldAsync<TField>(T item, Expression<Func<T, TField>> predicate)
        {
            db.Attach(item);
            db.Entry(item).Property(predicate).IsModified = true;
            await db.SaveChangesAsync();
            return true;
        }
    }
}
