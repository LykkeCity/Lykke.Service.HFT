using System.ComponentModel.DataAnnotations;
using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Middleware;

namespace Lykke.Service.HFT.Models.Requests
{
    public class LimitOrderRequest
    {
        [Required]
        public string AssetPairId { get; set; }
        [Required]
        public OrderAction OrderAction { get; set; }
        [Required, DoubleGreaterThanZero]
        public double Volume { get; set; }
        [Required, DoubleGreaterThanZeroAttribute]
        public double Price { get; set; }
    }
}
