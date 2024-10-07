using System;
using System.Threading;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Network.Extensions
{
    internal static class RandomDelayStaticProvider
    {
        internal static async Task DelayAsync(int delayTimeInMs, int diffInMs)
        {
            var randomTimeDiff = new Random().Next(diffInMs);
            await Task.Delay(TimeSpan.FromMilliseconds(delayTimeInMs + randomTimeDiff), CancellationToken.None);
        }
    }
}
