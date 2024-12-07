using AutoMapper;
using SteamDigiSellerBot.Database.Entities;
using SteamDigiSellerBot.Utilities.Services;

namespace SteamDigiSellerBot.MappingProfiles
{
    public class ProxyMappingProfile : Profile
    {
        public ProxyMappingProfile()
        {
            CreateMap<string, SteamProxy>().AfterMap<ProxyMappingAction>();
        }
    }

    public class ProxyMappingAction : IMappingAction<string, SteamProxy>
    {
        public void Process(string source, SteamProxy destination, ResolutionContext context)
        {
            string[] proxyData = source.Split(new char[] { ':', ';' });

            if (proxyData.Length == 2)
            {
                destination.Host = proxyData[0];
                destination.Port = int.Parse(proxyData[1]);
            }

            if (proxyData.Length == 4)
            {
                destination.Host = proxyData[0];
                destination.Port = int.Parse(proxyData[1]);
                destination.UserName = proxyData[2];
                destination.Password = CryptographyUtilityService.Encrypt(proxyData[3]);
            }
        }
    }
}
