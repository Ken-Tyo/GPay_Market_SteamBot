using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Utilities.Services
{
    public interface IWsNotificationSender
    {
        //Task SendNotification<T>(string user, T data);
        //Task SendNotificationByUserId<T>(int userId, T data);
        Task GameSessionChanged(string aspUserId, int gsId);
        Task GameSessionChangedAsync(string uniqueCode);
    }
}
