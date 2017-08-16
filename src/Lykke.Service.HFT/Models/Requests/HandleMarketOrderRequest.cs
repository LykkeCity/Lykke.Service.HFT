using System.ComponentModel.DataAnnotations;
using Lykke.Service.HFT.Middleware;

namespace Lykke.Service.HFT.Models.Requests
{
    public class HandleMarketOrderRequest
	{
		[Required]
		public string AssetPair { get; set; }
		[Required, NonZero]
		public double Volume { get; set; }
		[Required]
		public bool Straight { get; set; }
	}
}
