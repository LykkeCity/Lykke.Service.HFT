using System.Threading.Tasks;
using Lykke.Sdk;

namespace Lykke.Service.HFT
{
    /// <summary>
    /// Program class
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Application entry point.
        /// </summary>
        public static Task Main(string[] args)
        {
#if DEBUG
            return LykkeStarter.Start<Startup>(true);
#else
            return LykkeStarter.Start<Startup>(false);
#endif
        }
    }
}
