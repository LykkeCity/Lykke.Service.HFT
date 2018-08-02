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
        /// Application entrypoint.
        /// </summary>
        public static Task Main(string[] args)
        {
            const int port = 5000;

#if DEBUG
            return LykkeStarter.Start<Startup>(true, port);
#else
            return LykkeStarter.Start<Startup>(true, port);
#endif
        }
    }
}
