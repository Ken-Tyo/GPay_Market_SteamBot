using System.Collections.Generic;

namespace SteamDigiSellerBot.Database.Entities.TagReplacements
{
    public interface ITagReplacement<T>
        where T : ITagReplacementValue
    {
        int Id { get; set; }

        ICollection<T> ReplacementValues { get; init; }
    }
}
