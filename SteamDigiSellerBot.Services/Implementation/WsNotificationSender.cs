using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using SteamDigiSellerBot.Database.Repositories;
using SteamDigiSellerBot.Hubs;
using SteamDigiSellerBot.Utilities.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Services.Implementation
{
    public class WsNotificationSender: IWsNotificationSender
    {
        private readonly IHubContext<AdminHub> adminHub;
        private readonly IHubContext<HomeHub> homeHub;
        private readonly IUserDBRepository _userDBRepository;
        private Dictionary<string, HashSet<string>> userUniqueCodes;
        private object sync = new object();

        public WsNotificationSender(
            IHubContext<AdminHub> adminHub,
            IHubContext<HomeHub> homeHub,
            IUserDBRepository userDbRepository)
        {
            this.adminHub = adminHub;
            this.homeHub = homeHub;
            _userDBRepository = userDbRepository;
            userUniqueCodes = new Dictionary<string, HashSet<string>>();
        }

        public void AddUniqueCode(string connId, string uc)
        {
            lock (sync)
            {
                if (!userUniqueCodes.ContainsKey(uc))
                    userUniqueCodes[uc] = new HashSet<string>();

                userUniqueCodes[uc].Add(connId);
            }
        }

        public async Task GameSessionChangedAsync(string uniqueCode)
        {
            await homeHub.Clients
                .Group(uniqueCode)
                .SendAsync("notify",
                new WsNotificationMes
                {
                    Type = WsNotificationType.GameSessionChanged,
                    Data = new { uniqueCode = uniqueCode }
                });
        }

        public async Task GameSessionChanged(string aspUserId, int gsId)
        {
            await SendNotificationByAspUserId(
                    aspUserId,
                    new WsNotificationMes
                    {
                        Type = WsNotificationType.GameSessionChanged,
                        Data = new { gsId = gsId }
                    });
        }

        private async Task SendNotification<T>(string aspUserName, T data)
        {
            var user = await _userDBRepository.GetByAspNetUserName(aspUserName);
            await adminHub.Clients
                .User(user.AspNetUser.Id).SendAsync("notify", data);
        }

        private async Task SendNotificationByUserId<T>(int userId, T data)
        {
            var user = await _userDBRepository.GetByIdAsync(userId);
            await adminHub.Clients
                .User(user.AspNetUser.Id).SendAsync("notify", data);
        }

        public async Task SendNotificationByAspUserId<T>(string aspUserId, T data)
        {
            await adminHub.Clients.User(aspUserId).SendAsync("notify", data);
        }
    }

    public class WsNotificationMes
    {
        public WsNotificationType Type { get; set; }
        public object Data { get; set; }
    }

    public enum WsNotificationType
    {
        GameSessionChanged = 1
    }
}
