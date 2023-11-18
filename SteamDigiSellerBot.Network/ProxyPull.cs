using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SteamDigiSellerBot.Database.Entities;
using SteamDigiSellerBot.Database.Repositories;
using SteamDigiSellerBot.Network.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Network
{
    public interface IProxyPull
    {
        SteamProxy GetFreeProxy();
        void RemoveProxy(params int[] proxyId);
        void RemoveAllProxy();
        void LoadNewProxy();
    }

    public class ProxyPull: IProxyPull
    {
        private readonly IServiceProvider _serviceProvider;
        private object sync = new object();
        private List<ProxyData> proxyData;
        private TimeSpan Timeout = TimeSpan.FromSeconds(90);
        private static Random random = new Random();
        private readonly ILogger<IProxyPull> _logger;

        public static readonly int MAX_REQUESTS = 110;

        public ProxyPull(
            IServiceProvider serviceProvider,
            ILogger<IProxyPull> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            Init();
        }

        private void Init()
        {
            lock (sync)
            {
                proxyData = new List<ProxyData>();
                var _proxyRepository = _serviceProvider
                    .CreateScope()
                    .ServiceProvider
                    .GetRequiredService<ISteamProxyRepository>();

                foreach (var p in _proxyRepository.ListAsync().Result)
                {
                    proxyData.Add(new ProxyData
                    {
                        SteamProxy = p
                    });
                }

                Task.Factory.StartNew(MonitorProxy);
            }
        }

        public void LoadNewProxy()
        {
            lock (sync)
            {
                var _proxyRepository = _serviceProvider
                    .CreateScope()
                    .ServiceProvider
                    .GetRequiredService<ISteamProxyRepository>();

                var proxyDict = proxyData.ToDictionary(pd => pd.SteamProxy.Id);
                foreach (var p in _proxyRepository.ListAsync().Result)
                {
                    if (!proxyDict.ContainsKey(p.Id))
                    {
                        proxyData.Add(new ProxyData
                        {
                            SteamProxy = p
                        });
                    }
                }
            }
        }

        public void RemoveAllProxy()
        {
            lock (sync)
            {
                proxyData = new List<ProxyData>();
            }
        }

        public void RemoveProxy(params int[] proxyId)
        {
            lock (sync)
            {
                var pl = proxyData.Where(p => proxyId.Contains(p.SteamProxy.Id)).ToList();
                foreach (var p in pl)
                    proxyData.Remove(p);
            }
        }

        public SteamProxy GetFreeProxy()
        {
            lock (sync)
            {
                var proxies = proxyData
                    .Where(p => p.TimeoutExp is null).ToList();

                if (proxies.Count == 0)
                    return null;

                var proxy = proxies[random.Next(proxies.Count)];
                proxy.UsingCount++;
                proxy.LastUsing = DateTime.Now;
                if (proxy.UsingCount > MAX_REQUESTS)
                    proxy.TimeoutExp = DateTime.Now.Add(Timeout);

                return proxy.SteamProxy;
            }
        }

        public async void MonitorProxy()
        {
            while (true)
            {
                var readyToUse = new List<dynamic>();
                lock (sync)
                {
                    try
                    {
                        foreach (var p in proxyData)
                        {
                            if (DateTime.Now > p.TimeoutExp || DateTime.Now - p.LastUsing > Timeout)
                            {
                                p.TimeoutExp = null;
                                p.UsingCount = 0;
                                p.LastUsing = DateTime.MaxValue;

                                readyToUse.Add(new { id = p.SteamProxy.Id, data = $"{p.SteamProxy.Host}:{p.SteamProxy.Port}" });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("MonitorProxy", ex);
                    }
                }

                if (readyToUse.Count > 0)
                {
                    Console.WriteLine(
                        $"\n------------------\nproxy reade to use: {readyToUse.Count}\n------------------\n");
                }
                await Task.Delay(5000);
            }
        }
    }

    public class ProxyData
    {
        public SteamProxy SteamProxy { get; set; }
        public int UsingCount { get; set; }
        public DateTime LastUsing { get; set; } = DateTime.MaxValue;
        public DateTime? TimeoutExp { get; set; }
    }
}
