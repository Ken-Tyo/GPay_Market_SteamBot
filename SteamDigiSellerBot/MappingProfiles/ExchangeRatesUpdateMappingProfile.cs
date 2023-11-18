using AutoMapper;
using SteamDigiSellerBot.Database.Entities;
using SteamDigiSellerBot.Models.ExchangeRates;

namespace SteamDigiSellerBot.MappingProfiles
{
    public class ExchangeRatesUpdateMappingProfile : Profile
    {
        public ExchangeRatesUpdateMappingProfile()
        {
            CreateMap<UpdateRatesRequest, CurrencyData>()
                .ForMember(x => x.Id, x => x.MapFrom(x => x.Id));

            CreateMap<UpdateRatesRequest.CurrencyValue, Currency>()
                .ForMember(x => x.Id, x => x.MapFrom(x => x.Id))
                .ForMember(x => x.Value, x => x.MapFrom((cs, ct) =>
                {
                    if (string.IsNullOrEmpty(cs.Value))
                        return -1;

                    var val = cs.Value.Replace(".", ",");

                    if (decimal.TryParse(val, out decimal res))
                        return res;

                    return -1;
                }));
        }
    }
}
