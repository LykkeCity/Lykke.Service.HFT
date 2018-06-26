using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.HFT.Contracts.Wallets;
using Refit;

namespace Lykke.Service.HFT.Client
{
    /// <summary>
    /// Service inteface for high-frequency trading wallets functionality.
    /// </summary>
    /// <remarks>
    /// Service expects api-key header for authentification. This can be send using a custom handler or the Lykke WithApiKey call on the client generator.
    /// </remarks>
    [PublicAPI]
    [Headers("api-key")]
    public interface IWalletsApi
    {
        /// <summary>
        /// Gets the asset balances of given api-key wallet.
        /// </summary>
        /// <param name="apiKey">The API key header. Can also be send using a custom handler or the Lykke WithApiKey call on the client generator.</param>
        [Get("/api/Wallets")]
        Task<IReadOnlyCollection<BalanceModel>> GetBalances([Header("api-key")] string apiKey = null);
    }
}