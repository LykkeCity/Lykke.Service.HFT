using AzureStorage.Tables;
using Common.Log;
using Lykke.Service.HFT.AzureRepositories.Accounts;
using Lykke.SettingsReader;

namespace Lykke.Service.HFT.AzureRepositories
{
	public static class AzureRepoFactories
	{
		public static WalletsRepository CreateAccountsRepository(IReloadingManager<string> connString, ILog log)
		{
			return new WalletsRepository(AzureTableStorage<WalletEntity>.Create(connString, "Accounts", log));
		}
	}
}
