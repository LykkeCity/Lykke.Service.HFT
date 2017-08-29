using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.HFT.Core.Accounts
{
	public interface IWallet
	{
		double Balance { get; }
		string AssetId { get; }
		double Reserved { get; }
	}

	public class Wallet : IWallet
	{
		public string AssetId { get; set; }
		public double Reserved { get; set; }
		public double Balance { get; set; }

		public static Wallet Create(string assetId, double balance = 0)
		{
			return new Wallet
			{
				AssetId = assetId,
				Balance = balance
			};
		}
	}

	public interface IWalletsRepository
	{
		Task<IEnumerable<IWallet>> GetAsync(string clientId);
		Task<IWallet> GetAsync(string clientId, string assetId);
		Task UpdateBalanceAsync(string clientId, string assetId, double balance);
		Task<Dictionary<string, double>> GetTotalBalancesAsync();

		Task GetWalletsByChunkAsync(Func<IEnumerable<KeyValuePair<string, IEnumerable<IWallet>>>, Task> chunk);
	}
}
