namespace SteamDigiSellerBot.Network
{
    internal static class NetworkConst
    {
        internal const int TriesCount = 10;
        internal const string ApplicationJsonContentType = "application/json";
        internal const int RequestRetryPauseDurationWithoutErrorInSeconds = 5;
        internal const int RequestRetryPauseDurationAfterErrorInSeconds = 7;
        internal const int RequestDelayInMs = 200;
    }
}
