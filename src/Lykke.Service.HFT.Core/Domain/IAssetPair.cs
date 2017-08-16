using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.HFT.Core.Domain
{
	public interface IAssetPair
	{
		string Id { get; }
		string Name { get; }
		string BaseAssetId { get; }
		string QuotingAssetId { get; }
		int Accuracy { get; }
		int InvertedAccuracy { get; }
		string Source { get; }
		string Source2 { get; }
		bool IsDisabled { get; }
	}

	public class AssetPair : IAssetPair
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public string BaseAssetId { get; set; }
		public string QuotingAssetId { get; set; }
		public int Accuracy { get; set; }
		public int InvertedAccuracy { get; set; }
		public string Source { get; set; }
		public string Source2 { get; set; }
		public bool IsDisabled { get; set; }

		public static AssetPair CreateDefault()
		{
			return new AssetPair
			{
				Accuracy = 5,
				IsDisabled = false
			};
		}
	}

	public interface IAssetPairsRepository
	{
		Task<IEnumerable<IAssetPair>> GetAllAsync();
		Task AddAsync(IAssetPair assetPair);
	}
}
