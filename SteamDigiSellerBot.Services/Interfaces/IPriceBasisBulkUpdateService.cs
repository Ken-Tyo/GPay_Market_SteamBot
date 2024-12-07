using System.Threading.Tasks;
using System.Threading;
using SteamDigiSellerBot.Database.Models;

namespace SteamDigiSellerBot.Services.Interfaces
{
    public sealed record PriceBasisBulkUpdateCommand(int? SteamCurrencyId, int[] Ids, User User);
    public interface IPriceBasisBulkUpdateService
    {
        Task UpdateAsync(PriceBasisBulkUpdateCommand bulkUpdateCommand, CancellationToken cancellationToken);
    }
}