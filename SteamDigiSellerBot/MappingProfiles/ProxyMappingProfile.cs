using AutoMapper;
using SteamDigiSellerBot.Database.Entities;

namespace SteamDigiSellerBot.MappingProfiles
{
    public class ProxyMappingProfile : Profile
    {
        public ProxyMappingProfile()
        {
            CreateMap<string, SteamProxy>().AfterMap((src, dst) =>
            {
                string[] proxyData = src.Split(new char[] { ':', ';' });

                if (proxyData.Length == 2)
                {
                    dst.Host = proxyData[0];
                    dst.Port = int.Parse(proxyData[1]);
                }

                if (proxyData.Length == 4)
                {
                    dst.Host = proxyData[0];
                    dst.Port = int.Parse(proxyData[1]);
                    dst.UserName = proxyData[2];
                    dst.Password = proxyData[3];
                }
            });
        }
    }
}
