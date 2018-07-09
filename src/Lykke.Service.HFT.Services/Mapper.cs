using System;
using Lykke.Service.HFT.Contracts.Orders;
using MeCancelMode = Lykke.MatchingEngine.Connector.Abstractions.Models.CancelMode;
using MeOrderAction = Lykke.MatchingEngine.Connector.Abstractions.Models.OrderAction;

namespace Lykke.Service.HFT.Services
{
    public static class Mapper
    {
        public static MeOrderAction ToMeOrderAction(this OrderAction action)
        {
            MeOrderAction orderAction;
            switch (action)
            {
                case OrderAction.Buy:
                    orderAction = MeOrderAction.Buy;
                    break;
                case OrderAction.Sell:
                    orderAction = MeOrderAction.Sell;
                    break;
                default:
                    throw new ArgumentException($"Unknown order action: {action}");
            }

            return orderAction;
        }

        public static MeCancelMode ToMeCancelModel(this CancelMode cancelMode)
        {
            switch (cancelMode)
            {
                case CancelMode.NotEmptySide:
                    return MeCancelMode.NotEmptySide;
                case CancelMode.BothSides:
                    return MeCancelMode.BothSides;
                case CancelMode.SellSide:
                    return MeCancelMode.SellSide;
                case CancelMode.BuySide:
                    return MeCancelMode.BuySide;
                default:
                    throw new ArgumentException($"Unknown cancel model: {cancelMode}");
            }
        }
    }
}
