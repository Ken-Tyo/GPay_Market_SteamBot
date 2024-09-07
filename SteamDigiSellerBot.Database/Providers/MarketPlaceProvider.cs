using Microsoft.EntityFrameworkCore;
using SteamDigiSellerBot.Database.Contexts;
using SteamDigiSellerBot.Database.Entities.TagReplacements;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Database.Providers
{
    public sealed class MarketPlaceProvider
    {
        private readonly DatabaseContext _databaseContext;

        public MarketPlaceProvider(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext ?? throw new ArgumentNullException(nameof(databaseContext));
        }

        public async Task<IReadOnlyList<MarketPlace>> GetAsync(CancellationToken cancellationToken) =>
            await _databaseContext.MarketPlaces.ToListAsync(cancellationToken);
    }
}
