using AzureStorage.Tables;
using Common.Log;
using Lykke.Service.HFT.AzureRepositories.Accounts;

namespace Lykke.Service.HFT.AzureRepositories
{
	public static class AzureRepoFactories
	{
		private const string TableNameDictionaries = "Dictionaries";

		public static AssetPairsRepository CreateAssetPairsRepository(string connString, ILog log)
		{
			return new AssetPairsRepository(new AzureTableStorage<AssetPairEntity>(connString, TableNameDictionaries, log));
		}
		public static WalletsRepository CreateAccountsRepository(string connString, ILog log)
		{
			return new WalletsRepository(new AzureTableStorage<WalletEntity>(connString, "Accounts", log));
		}
	}
}
