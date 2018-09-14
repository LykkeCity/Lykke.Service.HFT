using AutoMapper;
using Lykke.Service.Assets.Contract.Events;

namespace AssetsCache
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<AssetCreatedEvent, Asset>();
            CreateMap<AssetUpdatedEvent, Asset>();
            CreateMap<AssetPairCreatedEvent, AssetPair>();
            CreateMap<AssetPairUpdatedEvent, AssetPair>();

            CreateMap<Lykke.Service.Assets.Client.Models.Asset, Asset>();
            CreateMap<Lykke.Service.Assets.Client.Models.AssetPair, AssetPair>();
        }
    }
}
