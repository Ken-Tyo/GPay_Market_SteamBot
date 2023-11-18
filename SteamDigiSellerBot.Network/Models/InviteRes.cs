using Newtonsoft.Json;
using System.Collections.Generic;

namespace SteamDigiSellerBot.Network.Models
{
    public class InviteRes
    {
        public List<string> invited { get; set; }
        public int success { get; set; }
        public List<int> failed_invites_result { get; set; }

        public string resRaw { get; set; }

        public static string GetErrDescription(int i)
        {
            var strMessage = "";
            switch (i)
            {
                case 25:
                    strMessage = "Could not invite user. Your friends list is full.";
                    break;

                case 15:
                    strMessage = "Could not invite user. Their friends list is full.";
                    break;

                case 40:
                    strMessage = "Error adding Friend. Communication between you and this user has been blocked.";
                    break;

                case 11:
                    strMessage = "You are blocking all communication with this user. Before communicating with this user, you must unblock them by visiting their Steam Community Profile.";
                    break;

                case 84:
                    strMessage = "It looks like you\"ve sent too many friend invites. To prevent spam, you\"ll have to wait before you can invite more friends. Please note that other players can still add you during this time.";
                    break;

                case 24:
                    strMessage = "Your account does not meet the requirements to use this feature.";
                    break;
                default:
                    break;
            }

            return strMessage;
        }
    }
}
