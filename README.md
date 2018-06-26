# Lykke.Service.HftInternalService

Lykke internal service for creation and management of high-frequency trading wallets and it's api-keys.

Nuget: https://www.nuget.org/packages/Lykke.Service.HFT.Client/

[Refit](https://github.com/reactiveui/refit) client can be generated using the [Lykke HttpClientGenerator](https://github.com/LykkeCity/Lykke.HttpClientGenerator) or the default Refit ```RestService```.

### Registering HFT services

Refit clients are preferably registerd in your DI framework as scoped instances or singletons. Multiple instances of the same REST client are not needed to have parallel functionality.

#### Lykke HttpClientGenerator with AutoFac

```csharp
containerBuilder
    .RegisterClient<IHighFrequencyTradingApi>(myHftUrl)
    .WithApiKey(myApiKey);
```

#### Default Refit RestService

```csharp
myContainer
    .RegisterInstance<IHighFrequencyTradingApi>(RestService.For<IHighFrequencyTradingApi>(myHftUrl));
```

**Note:** Send your api-key as api-key header or as argument to the specific calls.

### Service endpoints

#### IsAlive

```csharp
public async Task<bool> IsHftAlive() {
    var client = container.Get<IHighFrequencyTradingApi>();
    return client.IsAlive();
}
```

#### Assetpairs

```csharp
var client = container.Get<IAssetPairsApi>();

// Get all asset pairs
var pairs = await client.GetAll();
pairs.Should().NotBeNull();

// Get a specific asset pair
var pair = await client.Get("BTCEUR");
pair.Should().NotBeNull();
```

#### History

```csharp
var client = container.Get<IHistoryApi>();

// Get a specific trade
var trade = await client.GetTrade(tradeId);
trade.Should().NotBeNull();

// Get latest trades by asset ID
var assetTrades = await client.GetLatestTrades("LKK");
assetTrades.Should().NotBeNull();

// Get latest trades by asset pair ID
var assetpairTrades = await client.GetLatestTradesByAssetPairId("BTCUSD");
assetpairTrades.Should().NotBeNull();

// Get latest 100 trades
var trades = await client.GetLatestTradesByAssetId(new LatestTradesQueryModel
            {
                AssetId = "BTC",
                Skip = 0,
                Take = 100
            });
trades.Should().NotBeNull();
```

#### Orderbooks

```csharp
var client = container.Get<IOrderBooksApi>();

// Get a specific order book
var orderbook = await client.Get("ETHCHF");
orderbook.Should().NotBeNull();

// Get all order books
var books = await client.GetAll();
books.Should().NotBeNull();
```

#### Wallets

```csharp
var client = container.Get<IWalletsApi>();

// Get wallet balances
var balances = await client.GetBalances();
balances.Should().NotBeNull();
```

### Orders

#### Get orders

```csharp
var client = container.Get<IOrdersApi>();

// Get a specific order
var order = await client.GetOrder(orderId);
order.Should().NotBeNull();

// Get all open orders
var openOrders = await client.GetOrders(OrderStatusQuery.Open);
openOrders.Should().NotBeNull();

// Get last 250 orders
var orders = await client.GetOrders(take: 250);
orders.Should().NotBeNull();
```

#### Market orders
```csharp
var client = container.Get<IOrdersApi>();
var order = new PlaceMarketOrderModel
{
    Asset = "BTC",
    AssetPairId = "BTCUSD",
    OrderAction = OrderAction.Buy,
    Volume = 0.001
};

// Place new market order
var result = await client.PlaceMarketOrder(order).TryExecute();

if (result.Success) {
    var price = result.Result.Result;
    price.Should().BeGreaterThan(0d);
} else {
    Console.WriteLine(result.Error);
}
```

#### Limit orders
```csharp
var client = container.Get<IOrdersApi>();
var order = new PlaceLimitOrderModel
{
    AssetPairId = "BTCUSD",
    OrderAction = OrderAction.Buy,
    Price = 500,
    Volume = 0.001
};

// Place new limit order
var result = await client.PlaceLimitOrder(order).TryExecute();
Guid orderId = Guid.Empty;

if (result.Success) {
    oderId = result.Result;
} else {
    Console.WriteLine(result.Error);
}

// Cancel limit order
await client.CancelLimitOrder(orderId);

// Cancel all open limit orders
await client.CancelAll();
```