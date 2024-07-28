using SteamDigiSellerBot.Services.Implementation.ItemBulkUpdateService;
using System.Threading;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Services.Interfaces
{
    public interface IItemBulkUpdateService
    {
        Task UpdateAsync(ItemBulkUpdateCommand bulkUpdateCommand, CancellationToken cancellationToken);
    }
}
