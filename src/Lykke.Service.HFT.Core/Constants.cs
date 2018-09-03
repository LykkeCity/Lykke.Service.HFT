namespace Lykke.Service.HFT.Core
{
    public static class Constants
    {
        public const string FinanceDataCacheInstance = "FinanceDataCacheInstance";
        public const string ApiKeyCacheInstance = "HftApiCache";
        private const string ApiKeyCacheKeyPattern = ":apiKey:{0}";
        private const string WalletCacheKeyPattern = ":wallet:{0}";
        private const string OrderBooksCacheKeyPattern = ":OrderBooks:{0}_{1}__";

        public static string GetKeyForApiKey(string apiKey)
        {
            return string.Format(ApiKeyCacheKeyPattern, apiKey);
        }

        public static string GetKeyForWalletId(string wallet)
        {
            return string.Format(WalletCacheKeyPattern, wallet);
        }

        public static string GetKeyForOrderBook(string assetPairId, bool isBuy)
        {
            return string.Format(OrderBooksCacheKeyPattern, assetPairId, isBuy);
        }
    }
}
