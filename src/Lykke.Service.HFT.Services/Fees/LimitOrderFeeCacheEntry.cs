using MessagePack;

namespace Lykke.Service.HFT.Services.Fees
{
    /// <summary>
    /// Limit order fee cache entry
    /// </summary>
    /// <remarks>MessagePack DTO</remarks>
    [MessagePackObject]
    public class LimitOrderFeeCacheEntry
    {
        [Key(0)]
        public int Type { get; set; }
        [Key(1)]
        public double MakerSize { get; set; }
        [Key(2)]
        public double TakerSize { get; set; }
        [Key(3)]
        public int MakerSizeType { get; set; }
        [Key(4)]
        public int TakerSizeType { get; set; }
        [Key(5)]
        public double MakerFeeModificator { get; set; }
    }
}