namespace SteamDigiSellerBot.Database.Entities.TagReplacements
{
    public interface ITagReplacementValue
    {
        string LanguageCode { get; }

        string Value { get; }
    }
}
