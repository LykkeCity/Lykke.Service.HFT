using AutoMapper;
using Lykke.Service.Assets.Contract.Events;
using Lykke.Service.HFT.Core.Domain;

namespace Lykke.Service.HFT
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<AssetCreatedEvent, Asset>();
            CreateMap<AssetUpdatedEvent, Asset>();
            CreateMap<AssetPairCreatedEvent, AssetPair>();
            CreateMap<AssetPairUpdatedEvent, AssetPair>();
        }
    }
}
