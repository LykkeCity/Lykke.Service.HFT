using System;
using System.Collections.Generic;
using System.Linq;

namespace Lykke.Service.HFT.Core.Domain
{
	public interface IOrderBook
	{
		string AssetPair { get; }
		bool IsBuy { get; }
		DateTime Timestamp { get; }
		List<VolumePrice> Prices { get; set; }
	}

	public class OrderBook : IOrderBook
	{
		public string AssetPair { get; set; }
		public bool IsBuy { get; set; }
		public DateTime Timestamp { get; set; }
		public List<VolumePrice> Prices { get; set; } = new List<VolumePrice>();
	}

	public class VolumePrice
	{
		public double Volume { get; set; }
		public double Price { get; set; }
    }

    public static class Ext
    {
        public static IOrderBook Invert(this IOrderBook orderBook)
        {
            foreach (var price in orderBook.Prices)
            {
                price.Volume = price.Volume * price.Price * -1;
            }

            return orderBook;
        }

        public static IOrderBook Order(this IOrderBook orderBook)
        {
            if (orderBook.IsBuy)
            {
                orderBook.Prices = orderBook.Prices.OrderByDescending(x => x.Price).ToList();
            }
            else
            {
                orderBook.Prices = orderBook.Prices.OrderBy(x => x.Price).ToList();
            }

            return orderBook;
        }
    }
}
