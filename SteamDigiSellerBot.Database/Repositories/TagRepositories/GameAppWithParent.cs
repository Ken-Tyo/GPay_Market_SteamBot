namespace SteamDigiSellerBot.Database.Repositories.TagRepositories
{
    public record GameAppWithParent : GameApp
    {
        public GameAppWithParent(string appId, string name) : base(appId, name)
        {
        }

        public GameApp ParentGameApp { get; set; }
    }
}
