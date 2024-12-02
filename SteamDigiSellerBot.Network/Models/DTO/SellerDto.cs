namespace SteamDigiSellerBot.Network.Models.DTO
{
    public class SellerDto
    {
        public int? Id { get; set; }

        public string Login { get; set; }

        public string Password { get; set; }

        public string UserId { get; set; }

        public int? RentDays { get; set; }
        
        public int? ItemsLimit { get; set; }

        public bool Blocked { get; set; }

        public bool PermissionDigisellerItems { get; set; }

        public bool PermissionKFGItems { get; set; }

        public bool PermissionFuryPayItems { get; set; }

        public bool PermissionItemsHierarchy { get; set; }

        public bool PermissionOneTimeBots { get; set; }

        public bool PermissionOrderSessionCreation { get; set; }

        public bool PermissionItemsMultiregion { get; set; }

        public bool PermissionDirectBotsDeposit { get; set; }

        public bool PermissionBotsLimitsParsing { get; set; }

        public bool PermissionDigisellerItemsGeneration { get; set; }
    }
}