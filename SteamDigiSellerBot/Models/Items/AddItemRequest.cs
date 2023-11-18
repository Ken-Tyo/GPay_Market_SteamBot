using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SteamDigiSellerBot.Models.Items
{
    public class AddItemRequest: IValidatableObject
    {
        [Required(ErrorMessage = "Поле AppId является обязательным")]
        public string AppId { get; set; }

        [Required(ErrorMessage = "Поле Издание является обязательным")]
        public string SubId { get; set; }

        [Required(ErrorMessage = "Поле DigiSeller Ids является обязательным")]
        public string DigiSellerIds { get; set; }

        //[Required(ErrorMessage = "Поле Процент от Steam является обязательным")]
        public decimal? SteamPercent { get; set; }

        [Required(ErrorMessage = "Поле DLC от стима является обязательным")]
        public bool IsDlc { get; set; }

        //[Required(ErrorMessage = "Поле Дополнительная цена является обязательным")]
        public decimal? AddPrice { get; set; }

        [Required(ErrorMessage = "Поле Ценовая основа является обязательным")]
        public int SteamCurrencyId { get; set; }

        
        public decimal? FixedDigiSellerPrice { get; set; }

        [Required(ErrorMessage = "Поле \"Тип установки цены\" является обязательным")]
        public bool IsFixedPrice { get; set; }

        public bool IsAutoActivation { get; set; }
        public int? MinActualThreshold { get; set; }
        [Required]
        public int SteamCountryCodeId { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            if (IsFixedPrice && FixedDigiSellerPrice is null) 
                results.Add(new ValidationResult("Поле Цена Digiseller является обязательным"));
            //if (IsFixedPrice && IsAutoActivation is null)
            //    results.Add(new ValidationResult("Поле Авто-активация является обязательным"));
            if (IsFixedPrice && MinActualThreshold is null)
                results.Add(new ValidationResult("Поле Мин. порог актуальности является обязательным"));

            if (!IsFixedPrice && SteamPercent is null)
                results.Add(new ValidationResult("Поле Процент от Steam является обязательным"));
            if (!IsFixedPrice && AddPrice is null)
                results.Add(new ValidationResult("Поле Дополнительная цена является обязательным"));

            return results;
        }
    }
}
