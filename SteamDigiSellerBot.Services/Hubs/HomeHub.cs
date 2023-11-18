using Microsoft.AspNetCore.SignalR;
using SteamDigiSellerBot.Utilities.Services;
using System;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Hubs
{
    public class HomeHub: Hub
    {
        public async Task SendUniqueCode(string code)
        {
            var conId = Context.ConnectionId;
            await Groups.AddToGroupAsync(conId, code);
        }
    }
}
