using System;

namespace Lykke.Service.HFT.Core.Domain
{
	public interface IHasId
	{
		Guid Id { get; set; }
	}
}
