using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Columns;
using BenchmarkDotNet.Attributes.Jobs;
using Lykke.Service.HFT.AutorestClient;
using Lykke.Service.HFT.AutorestClient.Models;
using System;
using System.Linq;

namespace Lykke.Service.HFT.Benchmarks
{
    /// <summary>
    /// Benchmark tests for the production HFT api.
    /// </summary>
    [RankColumn]
    [SimpleJob(launchCount: 1, warmupCount: 1, targetCount: 5, invocationCount: 5, id: "QuickJob")]
    public class HftProductionBenchmark
    {
        //private const string HftApiUrl = @"https://hft-api.lykke.com/";
        private const string HftApiUrl = @"http://localhost:5000";
        private const string DevApiKey = @"9b93e3ec-f9b0-42ba-b68c-cecb0d1e6dd4";
        private IHighFrequencytradingAPI _client;
        private string[] _pairs;
        private Random _randomPairIndex;

        [GlobalSetup]
        public void Setup()
        {
            _client = new HighFrequencytradingAPI();
            _client.BaseUri = new Uri(HftApiUrl);

            _pairs = _client
                .GetAssetPairs()
                .Select(x => x.Id)
                .ToArray();

            _randomPairIndex = new Random();
        }

        public string GetRandomAssetPairId()
        {
            var index = _randomPairIndex.Next(0, _pairs.Length - 1);
            return _pairs[index];
        }

        [Benchmark]
        public void IsAlive()
        {
            _client.IsAlive();
        }

        [Benchmark]
        public void GetAssetPairs()
        {
            _client.GetAssetPairs();
        }

        [Benchmark]
        public void GetPair()
        {
            _client.GetAssetPair(GetRandomAssetPairId());
        }

        [Benchmark]
        public void GetOrderBooks()
        {
            _client.GetOrderBooks();
        }

        [Benchmark]
        public void GetOrderBook()
        {
            _client.GetOrderBook(GetRandomAssetPairId());
        }

        [Benchmark]
        public void PlaceLimitAndCancelOrder()
        {
            var order = new LimitOrderRequest
            {
                AssetPairId = "BTCUSD",
                OrderAction = OrderAction.Buy,
                Price = 500,
                Volume = 0.001
            };

            var result = _client.PlaceLimitOrder(DevApiKey, order);

            switch(result)
            {
                case ResponseModel error:
                    Console.WriteLine(error.Error?.Message);
                    break;
                case Guid orderId:
                    _client.CancelLimitOrder(orderId, DevApiKey);
                    break;
                default:
                    Console.WriteLine(result);
                    break;
            }            
        }
    }
}
