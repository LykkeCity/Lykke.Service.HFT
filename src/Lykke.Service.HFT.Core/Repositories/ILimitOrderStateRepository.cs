using System;
using System.Threading.Tasks;
using Lykke.Service.HFT.Core.Domain;

namespace Lykke.Service.HFT.Core.Repositories
{
    public interface ILimitOrderStateRepository
    {
        Task<ILimitOrderState> GetAsync(string clientId, Guid orderId);
    }
}