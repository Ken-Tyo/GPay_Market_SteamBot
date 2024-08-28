namespace SteamDigiSellerBot.Network
{
    internal static class NetworkConst
    {
        internal const int TriesCount = 10;
        internal const string ApplicationJsonContentType = "application/json";
        internal const int RequestRetryPauseDurationAfterErrorInSeconds = 5;
        internal const int RequestDelayInMs = 200;
    }
}
