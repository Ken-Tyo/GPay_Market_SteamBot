using SteamDigiSellerBot.Network.Models.UpdateItemInfoCommand;
using System.Threading;

namespace SteamDigiSellerBot.Network.Services.Hangfire
{
    public sealed record HangfireUpdateItemInfoJobCommand(UpdateItemInfoCommands UpdateItemInfoCommands, string AspNetUserId);
}
