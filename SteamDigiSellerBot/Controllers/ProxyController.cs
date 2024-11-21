using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SteamDigiSellerBot.ActionFilters;
using SteamDigiSellerBot.Database.Entities;
using SteamDigiSellerBot.Database.Repositories;
using SteamDigiSellerBot.Models.Proxy;
using SteamDigiSellerBot.Network;
using SteamDigiSellerBot.Utilities.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Controllers
{
    [Authorize]
    public class ProxyController : Controller
    {
        private readonly ISteamProxyRepository _steamProxyRepository;
        private readonly IProxyPull _proxyPull;
        private readonly IMapper _mapper;

        public ProxyController(
            ISteamProxyRepository steamProxyRepository, 
            IProxyPull proxyPull,
            IMapper mapper)
        {
            _steamProxyRepository = steamProxyRepository;
            _proxyPull = proxyPull;
            _mapper = mapper;
        }

        [HttpGet, Route("proxy/list")]
        public async Task<IActionResult> ProxyList()
        {
            List<SteamProxy> proxies = await _steamProxyRepository.ListAsync();
            proxies.ForEach(e =>
            {
                e.Password = CryptographyUtilityService.Decrypt(e.Password);
            });

            return Ok(proxies);
        }

        [HttpGet, Route("proxy/delete")]
        public async Task<IActionResult> ProxyDelete(int id)
        {
            if (id > 0)
            {
                SteamProxy proxy = await _steamProxyRepository.GetByIdAsync(id);

                if (proxy != null)
                {
                    await _steamProxyRepository.DeleteAsync(proxy);
                    _proxyPull.RemoveProxy(proxy.Id);
                    return Ok();
                }
            }

            return BadRequest();
        }

        [HttpGet, Route("proxy/delete/all")]
        public async Task<IActionResult> ProxiesDeleteAll()
        {
            var pl = await _steamProxyRepository.ListAsync();
            await _steamProxyRepository.DeleteListAsync(pl);
            _proxyPull.RemoveAllProxy();

            return Ok();
        }

        [HttpPost, Route("proxy/load"), ValidationActionFilter]
        public async Task<IActionResult> ProxyLoad(LoadProxiesRequest request)
        {
            var strList = request.Proxies.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            List<SteamProxy> proxies = _mapper.Map<List<SteamProxy>>(strList);

            await _steamProxyRepository.AddRangeAsync(proxies);
            _proxyPull.LoadNewProxy();

            return Ok();
        }
    }
}
