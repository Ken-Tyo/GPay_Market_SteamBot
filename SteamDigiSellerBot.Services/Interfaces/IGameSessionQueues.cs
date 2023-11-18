namespace SteamDigiSellerBot.Services.Interfaces
{
    public interface IGameSessionQueues
    {
        void AddToFriend(int gsId);
        bool AddToFriendDone(int id);
        bool AddToFriendInProgress(int id);
        int NextAddToFriend();
        void DoneAddToFriend(int gsId);
        int GetToAddToFriend(int id);
    }
}
