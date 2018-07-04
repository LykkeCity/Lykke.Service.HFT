using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.HFT.Contracts.OrderBook;

namespace Lykke.Service.HFT.Core.Services
{
    public interface IOrderBooksService
    {
        Task<ICollection<Guid>> GetOrderIdsAsync(IEnumerable<string> assetPairs);
        Task<IEnumerable<OrderBookModel>> GetAllAsync();
        Task<IEnumerable<OrderBookModel>> GetAsync(string assetPairId);
    }
}
