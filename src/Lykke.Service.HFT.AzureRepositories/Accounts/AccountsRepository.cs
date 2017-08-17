using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Common;
using Lykke.Service.HFT.Core.Accounts;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace Lykke.Service.HFT.AzureRepositories.Accounts
{

	public class WalletEntity : TableEntity
	{
		public class TheWallet : IWallet
		{
			[JsonProperty("balance")]
			public double Balance { get; set; }

			[JsonProperty("asset")]
			public string AssetId { get; set; }


			public static TheWallet Create(string assetId, double balance)
			{
				return new TheWallet
				{
					AssetId = assetId,
					Balance = balance
				};
			}
		}

		public static string GeneratePartitionKey()
		{
			return "ClientBalance";
		}

		public static string GenerateRowKey(string traderId)
		{
			return traderId;
		}

		public string ClientId => RowKey;

		public string Balances { get; set; }

		internal void UpdateBalance(string assetId, double balanceDelta)
		{
			var data = Get();
			var element = data.FirstOrDefault(itm => itm.AssetId == assetId);

			if (element != null)
			{
				element.Balance += balanceDelta;
				Balances = data.ToJson();
				return;
			}

			var list = new List<TheWallet>();
			list.AddRange(data);
			list.Add(TheWallet.Create(assetId, balanceDelta));
			Balances = list.ToJson();

		}

		internal static readonly TheWallet[] EmptyList = new TheWallet[0];

		internal TheWallet[] Get()
		{
			if (string.IsNullOrEmpty(Balances))
				return EmptyList;

			return Balances.DeserializeJson(() => EmptyList);
		}
		public static WalletEntity Create(string clientId)
		{
			return new WalletEntity
			{
				PartitionKey = GeneratePartitionKey(),
				RowKey = GenerateRowKey(clientId),

			};
		}
	}

	public class WalletsRepository : IWalletsRepository
	{
		private readonly INoSQLTableStorage<WalletEntity> _tableStorage;



		public WalletsRepository(INoSQLTableStorage<WalletEntity> tableStorage)
		{
			_tableStorage = tableStorage;
		}

		public async Task<IEnumerable<IWallet>> GetAsync(string traderId)
		{
			var partitionKey = WalletEntity.GeneratePartitionKey();
			var rowKey = WalletEntity.GenerateRowKey(traderId);
			var entity = await _tableStorage.GetDataAsync(partitionKey, rowKey);

			return entity == null
				? WalletEntity.EmptyList
				: entity.Get();
		}

		public async Task<IWallet> GetAsync(string traderId, string assetId)
		{
			var entities = await GetAsync(traderId);
			return entities.FirstOrDefault(itm => itm.AssetId == assetId);
		}

		public Task UpdateBalanceAsync(string traderId, string assetId, double balance)
		{
			var partitionKey = WalletEntity.GeneratePartitionKey();
			var rowKey = WalletEntity.GenerateRowKey(assetId);

			return _tableStorage.InsertOrModifyAsync(partitionKey, rowKey,

				() =>
				{
					var newEntity = WalletEntity.Create(traderId);
					newEntity.UpdateBalance(assetId, balance);
					return newEntity;
				},

				entity =>
				{
					entity.UpdateBalance(assetId, balance);
					return entity;
				}

			);
		}

		public async Task<Dictionary<string, double>> GetTotalBalancesAsync()
		{
			var result = new Dictionary<string, double>();

			await _tableStorage.GetDataByChunksAsync(entities =>
			{
				foreach (var walletEntity in entities)
				foreach (var balances in walletEntity.Get())
				{
					if (!result.ContainsKey(balances.AssetId))
						result.Add(balances.AssetId, balances.Balance);
					else
						result[balances.AssetId] += balances.Balance;
				}
			});

			return result;
		}

		public async Task GetWalletsByChunkAsync(Func<IEnumerable<KeyValuePair<string, IEnumerable<IWallet>>>, Task> chunkCallback)
		{

			await _tableStorage.GetDataByChunksAsync(async chunk =>
			{
				var yeldResult = new List<KeyValuePair<string, IEnumerable<IWallet>>>();

				foreach (var walletEntity in chunk)
				{
					var wallets = walletEntity.Get().Where(itm => itm.Balance != 0).ToArray();
					if (wallets.Length > 0)
						yeldResult.Add(new KeyValuePair<string, IEnumerable<IWallet>>(walletEntity.ClientId, wallets));
				}

				if (yeldResult.Count > 0)
				{
					await chunkCallback(yeldResult);
					yeldResult.Clear();
				}

			});
		}
	}
}
