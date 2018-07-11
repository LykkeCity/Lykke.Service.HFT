using System.Threading.Tasks;
using Lykke.Sdk;

namespace Lykke.Service.HFT
{
    public static class Program
    {
        public static Task Main(string[] args)
            => LykkeStarter.Start<Startup>(Core.Constants.ComponentName);
    }
}
