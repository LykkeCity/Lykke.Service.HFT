using AzureStorage.Tables;
using Common.Log;

namespace Lykke.Service.HFT.AzureRepositories
{
	public static class AzureRepoFactories
	{
		private const string TableNameDictionaries = "Dictionaries";
		

		public static AssetPairsRepository CreateAssetPairsRepository(string connString, ILog log)
		{
			return new AssetPairsRepository(new AzureTableStorage<AssetPairEntity>(connString, TableNameDictionaries, log));
		}
	}
}
