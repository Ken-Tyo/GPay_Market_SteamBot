using System;
using System.Collections.Generic;
using System.Text;
using SteamKit2.Internal;

namespace SteamKit2.Steam.Authentication.Result
{
    public class Confirmation
    {
        public int ConfirmationType { get; set; }
        public string AssociatedMessage { get; set; }
    }

    public class ResponseX
    {
        public ulong ClientId { get; set; }
        public string RequestId { get; set; }
        public double Interval { get; set; }
        public List<Confirmation> AllowedConfirmations { get; set; }
        public ulong SteamId { get; set; }
        public string WeakToken { get; set; }
        public string ExtendedErrorMessage { get; set; }
    }

    public class SteamID
    {
        public bool IsBlankAnonAccount { get; set; }
        public bool IsGameServerAccount { get; set; }
        public bool IsPersistentGameServerAccount { get; set; }
        public bool IsAnonGameServerAccount { get; set; }
        public bool IsContentServerAccount { get; set; }
        public bool IsClanAccount { get; set; }
        public bool IsChatAccount { get; set; }
        public bool IsLobby { get; set; }
        public bool IsIndividualAccount { get; set; }
        public bool IsAnonAccount { get; set; }
        public bool IsAnonUserAccount { get; set; }
        public bool IsConsoleUserAccount { get; set; }
        public bool IsValid { get; set; }
        public int AccountID { get; set; }
        public int AccountInstance { get; set; }
        public int AccountType { get; set; }
        public int AccountUniverse { get; set; }
    }

    public class LogInData
    {
        public CAuthentication_BeginAuthSessionViaCredentials_Response Responsex { get; set; }
        public SteamID SteamID { get; set; }
        public ulong ClientID { get; set; }
        public string RequestID { get; set; }
        public string PollingInterval { get; set; }
    }
}
