using SteamKit2.Authentication;

namespace SteamKit2.Steam.Authentication
{
    public class ResultAuthSession
    {
        public EResult Result { get;  set; }
        public CredentialsAuthSession CredentialsAuthSession {  get;  set; }
    }
}
