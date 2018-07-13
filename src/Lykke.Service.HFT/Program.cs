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
            => LykkeStarter.Start<Startup>(Core.Constants.ComponentName);
    }
}
