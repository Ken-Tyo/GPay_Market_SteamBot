using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SteamDigiSellerBot.Database.Providers;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace SteamDigiSellerBot.Controllers
{
    [ApiController]
    [Authorize]
    public class LanguageController : Controller
    {
        private readonly LanguageProvider _languageProvider;

        public LanguageController(LanguageProvider languageProvider)
        {
            _languageProvider = languageProvider
                ?? throw new ArgumentNullException(nameof(languageProvider));
        }

        [HttpGet("language")]
        public async Task<IActionResult> GetAsync(CancellationToken cancellationToken)
        {
            var result = await _languageProvider.GetAsync(cancellationToken);

            return Ok(result);
        }
    }
}
