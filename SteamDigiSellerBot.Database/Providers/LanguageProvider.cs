using Microsoft.EntityFrameworkCore;
using SteamDigiSellerBot.Database.Contexts;
using SteamDigiSellerBot.Database.Entities.Templates;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Database.Providers
{
    public sealed class LanguageProvider
    {
        private readonly DatabaseContext _databaseContext;

        public LanguageProvider(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext ?? throw new ArgumentNullException(nameof(databaseContext));
        }

        public async Task<IReadOnlyList<Language>> GetAsync(CancellationToken cancellationToken) =>
            await _databaseContext.Languages.ToListAsync(cancellationToken);
    }
}
