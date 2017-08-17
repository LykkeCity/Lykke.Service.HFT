using AzureStorage.Tables;
using Common.Log;
using Lykke.Service.HFT.AzureRepositories.Accounts;

namespace Lykke.Service.HFT.AzureRepositories
{
	public static class AzureRepoFactories
	{
		public static WalletsRepository CreateAccountsRepository(string connString, ILog log)
		{
			return new WalletsRepository(new AzureTableStorage<WalletEntity>(connString, "Accounts", log));
		}
	}
}
