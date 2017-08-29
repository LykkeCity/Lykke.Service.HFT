using System;
using Common.Log;

namespace Lykke.Service.HFT.Client
{
    public class HFTClient : IHFTClient, IDisposable
    {
        private readonly ILog _log;

        public HFTClient(string serviceUrl, ILog log)
        {
            _log = log;
        }

        public void Dispose()
        {
            //if (_service == null)
            //    return;
            //_service.Dispose();
            //_service = null;
        }
    }
}
