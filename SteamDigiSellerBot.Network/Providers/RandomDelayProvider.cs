using System;
using System.Threading;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Network.Providers
{
    public interface IRandomDelayProvider
    {
        Task DelayAsync(int delayTimeInMs, int diffInMs);
    }

    public sealed class RandomDelayProvider : IRandomDelayProvider
    {
        public async Task DelayAsync(int delayTimeInMs, int diffInMs)
        {
            var randomTimeDiff = new Random().Next(diffInMs);
            await Task.Delay(TimeSpan.FromMilliseconds(delayTimeInMs + randomTimeDiff), CancellationToken.None);
        }
    }
}
