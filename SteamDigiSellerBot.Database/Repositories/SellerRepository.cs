using System.Threading;
using System.Threading.Tasks;
using DatabaseRepository.Repositories;
using Microsoft.EntityFrameworkCore;
using SteamDigiSellerBot.Database.Contexts;
using SteamDigiSellerBot.Database.Entities;

namespace SteamDigiSellerBot.Database.Repositories
{
    public interface ISellerRepository : IBaseRepository<Seller>
    {
        Task Updatesync(Seller seller, CancellationToken cancellationToken = default);
    }

    public class SellerRepository : BaseRepository<Seller>, ISellerRepository
    {
        private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;

        public SellerRepository(IDbContextFactory<DatabaseContext> dbContextFactory)
            : base(dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task Updatesync(Seller seller, CancellationToken cancellationToken = default)
        {
            await using var databaseContext = _dbContextFactory.CreateDbContext();
            databaseContext.Sellers.Update(seller);
            await databaseContext.SaveChangesAsync(cancellationToken);
        }

    }
}
