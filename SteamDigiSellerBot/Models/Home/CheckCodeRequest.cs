namespace SteamDigiSellerBot.Models.Home
{
    public class CheckCodeRequest
    {
        public string Uniquecode { get; set; } = "";
        public string Seller_id { get; set; } = "";

        public string Captcha { get; set; }
    }
}
