using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using FluentAssertions;
using Lykke.HttpClientGenerator;
using Lykke.Service.HFT.Client;
using Lykke.Service.HFT.Contracts.Assets;
using Lykke.Service.HFT.Contracts.History;
using Lykke.Service.HFT.Contracts.Orders;
using Newtonsoft.Json;

namespace Lykke.Service.HFT.Benchmarks
{
    /// <summary>
    /// Benchmark tests for the production HFT api.
    /// </summary>
    [RankColumn]
    [SimpleJob(launchCount: 1, warmupCount: 1, targetCount: 2, invocationCount: 2, id: "QuickJob")]
    public class HftProductionBenchmark
    {
        private AssetPairModel[] _pairs;
        private HistoryTradeModel[] _trades;
        private LimitOrderStateModel[] _orders;
        private OrderAction[] _actions;
        private Random _random;

        private IHttpClientGenerator _generator;
        private readonly List<object> _clients = new List<object>();

        [GlobalSetup]
        public void Setup()
        {
            var json = File.ReadAllText(@"appsettings.bm.json");
            var settings = JsonConvert.DeserializeObject<BenchmarkSettings>(json);

            _generator = HttpClientGenerator.HttpClientGenerator
                .BuildForUrl(settings.HftUrl)
                .WithApiKey(settings.ApiKey)
                .WithoutRetries()
                .WithoutCaching()
                .Create();

            _pairs = GetClient<IAssetPairsApi>()
                .GetAll()
                .GetAwaiter()
                .GetResult()
                .ToArray();

            _trades = GetClient<IHistoryApi>()
                .GetLatestTradesByAssetId("BTC")
                .GetAwaiter()
                .GetResult()
                .ToArray();

            _orders = GetClient<IOrdersApi>()
                .GetOrders()
                .GetAwaiter()
                .GetResult()
                .ToArray();

            _actions = Enum
                .GetValues(typeof(OrderAction))
                .Cast<OrderAction>()
                .ToArray();

            _random = new Random();
        }

        [GlobalCleanup]
        public void Cleanup() => CancelAll().Wait();

        private T GetClient<T>()
        {
            var client = _clients.OfType<T>().FirstOrDefault();
            if (client == null)
            {
                client = _generator.Generate<T>();
                _clients.Add(client);
            }

            return client;
        }

        public T GetRandomItem<T>(ICollection<T> items)
        {
            var index = _random.Next(0, items.Count - 1);
            return items.ElementAt(index);
        }

        [Benchmark]
        public async Task IsAlive()
        {
            var client = GetClient<IHighFrequencyTradingApi>();
            var result = await client.GetIsAliveDetails().TryExecute();
            result.Should().NotBeNull();
        }

        [Benchmark]
        public async Task GetBalances()
        {
            var client = GetClient<IWalletsApi>();
            var result = await client.GetBalances();
            result.Should().NotBeNull();
        }

        [Benchmark]
        public async Task GetAssetPairs()
        {
            var client = GetClient<IAssetPairsApi>();
            var result = await client.GetAll();
            result.Should().NotBeNull();
        }

        [Benchmark]
        public async Task GetPair()
        {
            var client = GetClient<IAssetPairsApi>();
            var result = await client.Get(GetRandomItem(_pairs).Id);
            result.Should().NotBeNull();
        }

        [Benchmark]
        public async Task GetTrade()
        {
            var client = GetClient<IHistoryApi>();
            var result = await client.GetTrade(GetRandomItem(_trades).Id);
            result.Should().NotBeNull();
        }

        [Benchmark]
        public async Task GetLatestTradesByAssetId()
        {
            var client = GetClient<IHistoryApi>();
            var result = await client.GetLatestTradesByAssetId(GetRandomItem(_pairs).QuotingAssetId);
            result.Should().NotBeNull();
        }

        [Benchmark]
        public async Task GetLatestTradesByAssetPairId()
        {
            var client = GetClient<IHistoryApi>();
            var pair = GetRandomItem(_pairs);
            var result = await client.GetLatestTradesByAssetPairId(pair.QuotingAssetId, pair.Id);
            result.Should().NotBeNull();
        }

        [Benchmark]
        public async Task GetLatestTrades()
        {
            var client = GetClient<IHistoryApi>();
            var pair = GetRandomItem(_pairs);
            var result = await client.GetLatestTrades(new LatestTradesQueryModel
            {
                AssetId = pair.QuotingAssetId,
                Skip = 1,
                Take = 10
            });
            result.Should().NotBeNull();
        }

        [Benchmark]
        public async Task GetOrderBooks()
        {
            var client = GetClient<IOrderBooksApi>();
            var result = await client.GetAll();
            result.Should().NotBeNull();
        }

        [Benchmark]
        public async Task GetOrderBook()
        {
            var client = GetClient<IOrderBooksApi>();
            var pair = GetRandomItem(_pairs);
            var result = await client.Get(pair.Id);
            result.Should().NotBeNull();
        }

        [Benchmark]
        public async Task GetOrder()
        {
            var client = GetClient<IOrdersApi>();
            var order = GetRandomItem(_orders);
            var result = await client.GetOrder(order.Id);
            result.Should().NotBeNull();
        }

        [Benchmark]
        public async Task GetOrders()
        {
            var client = GetClient<IOrdersApi>();
            var result = await client.GetOrders();
            result.Should().NotBeNull();
        }

        [Benchmark]
        public async Task PlaceMarketOrder()
        {
            var client = GetClient<IOrdersApi>();
            var order = new PlaceMarketOrderModel
            {
                Asset = "BTC",
                AssetPairId = "BTCUSD",
                OrderAction = GetRandomItem(_actions),
                Volume = 0.001
            };

            var result = await client.PlaceMarketOrder(order).TryExecute();

            if (result.Success)
            {
                result.Result.Price.Should().BeGreaterThan(0d);
            }
            else
            {
                result.Error.Should().NotBeNull();
            }
        }

        [Benchmark]
        public async Task PlaceLimitAndCancelOrder()
        {
            var client = GetClient<IOrdersApi>();
            var order = new PlaceLimitOrderModel
            {
                AssetPairId = "BTCUSD",
                OrderAction = GetRandomItem(_actions),
                Price = 500,
                Volume = 0.001
            };

            var result = await client.PlaceLimitOrder(order).TryExecute();

            if (result.Success)
            {
                result.Result.Id.Should().NotBe(Guid.Empty);
            }
            else
            {
                result.Error.Should().NotBeNull();
            }

            if (result.Success)
            {
                await client.CancelLimitOrder(result.Result.Id);
            }
        }

        [Benchmark]
        public async Task PlaceBulkOrder()
        {
            var client = GetClient<IOrdersApi>();
            var order = new PlaceBulkOrderModel
            {
                AssetPairId = "BTCUSD",
                CancelPreviousOrders = true,
                Orders = Enumerable.Range(0,10).Select(x => new BulkOrderItemModel
                {
                    OrderAction = OrderAction.Buy,
                    Price = 500 + x,
                    Volume = 0.001
                })
            };

            var result = await client.PlaceBulkOrder(order).TryExecute();

            if (result.Success)
            {
                result.Result.Should().NotBeNull();
                result.Result.Statuses.Should().HaveCount(10);
                result.Result.Statuses.Should().NotContain(x => x.Id == Guid.Empty);
            }
            else
            {
                result.Error.Should().NotBeNull();
            }
        }

        [Benchmark]
        public async Task CancelAll()
        {
            var client = GetClient<IOrdersApi>();
            await client.CancelAll();
        }

        [Benchmark]
        public async Task ReplaceBulkOrder()
        {
            var assetPair = "BTCUSD";
            var client = GetClient<IOrdersApi>();

            await client.CancelAll();

            var order = new PlaceBulkOrderModel
            {
                AssetPairId = assetPair,
                Orders = Enumerable.Range(0, 2).Select(x => new BulkOrderItemModel
                {
                    OrderAction = OrderAction.Buy,
                    Price = 500 + x,
                    Volume = 0.0001
                })
            };

            var first = await client.PlaceBulkOrder(order);
            await Task.Delay(TimeSpan.FromSeconds(1));

            order = new PlaceBulkOrderModel
            {
                AssetPairId = assetPair,
                Orders = first.Statuses
                    .Where(x => x.Error == null)
                    .Select(x => new BulkOrderItemModel
                    {
                        OrderAction = OrderAction.Buy,
                        OldId = x.Id.ToString(),
                        Price = 1d + x.Price,
                        Volume = 0.0001
                    })
            };

            var result = await client.PlaceBulkOrder(order);
            await Task.Delay(TimeSpan.FromSeconds(1));

            foreach (var old in first.Statuses)
            {
                var current = await client.GetOrder(old.Id);
                current.Status.Should().Be(OrderStatus.Replaced);
            }

            foreach (var item in result.Statuses)
            {
                var current = await client.GetOrder(item.Id);
                current.Status.Should().Be(OrderStatus.InOrderBook);
            }
        }
    }
}
