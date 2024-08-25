using Microsoft.EntityFrameworkCore;
using SteamDigiSellerBot.Database.Contexts;
using SteamDigiSellerBot.Database.Entities.Templates;
using System.Threading;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Database.Repositories
{
    public interface IItemInfoTemplateRepository : IBaseRepositoryEx<ItemInfoTemplate>
    {
        Task<ItemInfoTemplate> AddAsync(ItemInfoTemplate itemInfoTemplate, CancellationToken cancellationToken);
    }

    public sealed class ItemInfoTemplateRepository : BaseRepositoryEx<ItemInfoTemplate>, IItemInfoTemplateRepository
    {
        public ItemInfoTemplateRepository(IDbContextFactory<DatabaseContext> dbContextFactory)
            : base(dbContextFactory)
        {
        }

        async Task<ItemInfoTemplate> IItemInfoTemplateRepository.AddAsync(ItemInfoTemplate itemInfoTemplate, CancellationToken cancellationToken)
        {
            await AddAsync(itemInfoTemplate, cancellationToken);

            return itemInfoTemplate;
        }
    }
}
