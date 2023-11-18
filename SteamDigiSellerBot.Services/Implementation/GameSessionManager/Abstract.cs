using System.Collections.Generic;
using System.Linq;

namespace SteamDigiSellerBot.Services.Implementation
{
    public abstract class BaseGameSessionManager
    {
        public virtual void Send(object data, GameSessionQueue sender)
        {

        }
    }

    public abstract class GameSessionQueue
    {
        protected BaseGameSessionManager manager;
        protected Dictionary<int, GsState> q;

        public Dictionary<int, GsState> Q => q;

        public GameSessionQueue(BaseGameSessionManager manager)
        {
            this.manager = manager;
            q = new Dictionary<int, GsState>();
        }

        public void SendToManager(object obj)
        {
            manager.Send(obj, this);
        }

        public void Add(int gsId)
        {
            q[gsId] = new GsState
            {
                Id = gsId,
                NumInQueue = q.Values.Count > 0
                    ? q.Values.ToList().Max(i => i.NumInQueue) + 1
                    : 1
            };
        }

        public bool Remove(int gsId)
        {
            var exist = q.ContainsKey(gsId);

            if (q.ContainsKey(gsId))
                q.Remove(gsId);

            return exist;
        }
    }
}
