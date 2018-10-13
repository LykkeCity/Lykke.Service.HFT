namespace Lykke.Service.HFT.Core
{
    public static class Constants
    {
        private const string ApiKeyCacheKeyPattern = ":apiKey:{0}";
        private const string WalletCacheKeyPattern = ":wallet:{0}";

        public static string GetKeyForApiKey(string apiKey)
        {
            return string.Format(ApiKeyCacheKeyPattern, apiKey);
        }

        public static string GetKeyForWalletId(string wallet)
        {
            return string.Format(WalletCacheKeyPattern, wallet);
        }
    }
}
