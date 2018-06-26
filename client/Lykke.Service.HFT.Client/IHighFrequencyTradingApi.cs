using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.HFT.Contracts.Health;
using Refit;

namespace Lykke.Service.HFT.Client
{
    /// <summary>
    /// Service interface to the high-frequency trading service.
    /// </summary>
    [PublicAPI]
    public interface IHighFrequencyTradingApi
    {
        /// <summary>
        /// Determines whether the high-frequency trading service is alive.
        /// </summary>
        [Get("/api/IsAlive")]
        Task<IsAliveModel> GetIsAliveDetails();
    }

    /// <summary>
    /// Extension methods for <see cref="IHighFrequencyTradingApi"/>.
    /// </summary>
    /// <remarks>TODO Move to default interface methods when supported in C# 8.0</remarks>
    [PublicAPI]
    public static class HftApiExtensions
    {
        /// <summary>
        /// Determines whether this high-frequency trading service is alive.
        /// </summary>
        /// <param name="api">The high-frequency trading API client.</param>
        /// <returns>[true] when alive, otherwise [false]</returns>
        public static async Task<bool> IsAlive(this IHighFrequencyTradingApi api)
        {
            try
            {
                var isAlive = await api.GetIsAliveDetails();
                return isAlive != null;
            }
            catch
            {
                return false;
            }
        }
    }
}