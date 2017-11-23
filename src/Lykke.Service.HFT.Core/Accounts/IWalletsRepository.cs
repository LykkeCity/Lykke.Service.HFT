using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.HFT.Core.Accounts
{
    public interface IWallet
    {
        double Balance { get; }
        string AssetId { get; }
        double Reserved { get; }
    }

    public class Wallet : IWallet
    {
        public double Balance { get; set; }
        public string AssetId { get; set; }
        public double Reserved { get; set; }
    }

    public interface IWalletsRepository
    {
        Task<IEnumerable<IWallet>> GetAsync(string clientId);
    }
}
