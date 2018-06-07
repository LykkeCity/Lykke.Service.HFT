using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.HFT.Core.Domain;

namespace Lykke.Service.HFT.Core.Repositories
{
    /// <summary>
    /// Archive for limit orders. Active limit orders can be found in <see cref="ILimitOrderStateRepository"/>.
    /// </summary>
    public interface ILimitOrderStateArchive
    {
        Task<ILimitOrderState> GetAsync(string clientId, Guid orderId);

        Task AddAsync(IEnumerable<ILimitOrderState> orders);
    }
}