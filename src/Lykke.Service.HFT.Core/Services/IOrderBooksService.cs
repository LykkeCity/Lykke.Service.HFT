using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.HFT.Core.Domain;

namespace Lykke.Service.HFT.Core.Services
{
    public interface IOrderBooksService
    {
        Task<IEnumerable<IOrderBook>> GetAllAsync();
        Task<IEnumerable<IOrderBook>> GetAsync(string assetPairId);
        Task<double?> GetBestPrice(string assetPair, bool buy);
    }
}
