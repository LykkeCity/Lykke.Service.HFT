using System.Collections.Generic;

namespace Lykke.Service.HFT.Services
{
    class OrderBookInternal
    {
        public class Order
        {
            public string Id { get; set; }
        }

        public List<Order> Prices { get; set; } = new List<Order>();
    }
}
