using System;
using System.Collections.Generic;

namespace SteamDigiSellerBot.Models.Items
{
    public class ProductsFilter
    {
        public bool IsFilterOn { get; set; }
        public string AppId { get; set; }

        public string ProductName { get; set; }

        public int? SteamCountryCodeId { get; set; }

        public string DigiSellerId { get; set; }

        public List<IdName> steamCurrencyId { get; set; }

        public List<IdName> gameRegionsCurrency { get; set; }

        public int? hierarchyParams_targetSteamCurrencyId { get; set; }

        public int? hierarchyParams_baseSteamCurrencyId { get; set; }

        public string hierarchyParams_compareSign { get; set; }

        public int? hierarchyParams_percentDiff { get; set; }

        public bool? hierarchyParams_isActiveHierarchyOn { get; set; }
    }

    public class IdName
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

}
