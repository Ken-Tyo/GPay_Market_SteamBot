namespace SteamDigiSellerBot.Database.Repositories.TagRepositories
{
    public record GameApp
    {
        public GameApp(string appId, string name)
        {
            AppId = appId;
            Name = name;
        }

        public string AppId { get; }
        
        public string Name { get; }
    }
}
