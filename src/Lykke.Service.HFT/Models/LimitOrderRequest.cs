namespace Lykke.Service.HFT.Models
{
    public class LimitOrderRequest
    {
	    public string AssetPairId { get; set; }
		public string OrderAction { get; set; }
		public double Volume { get; set; }
		public double Price { get; set; }
	}
}
