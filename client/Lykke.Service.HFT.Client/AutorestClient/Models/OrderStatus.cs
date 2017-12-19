// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace Lykke.Service.HFT.AutorestClient.Models
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using System.Runtime;
    using System.Runtime.Serialization;

    /// <summary>
    /// Defines values for OrderStatus.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OrderStatus
    {
        [EnumMember(Value = "Pending")]
        Pending,
        [EnumMember(Value = "InOrderBook")]
        InOrderBook,
        [EnumMember(Value = "Processing")]
        Processing,
        [EnumMember(Value = "Matched")]
        Matched,
        [EnumMember(Value = "NotEnoughFunds")]
        NotEnoughFunds,
        [EnumMember(Value = "NoLiquidity")]
        NoLiquidity,
        [EnumMember(Value = "UnknownAsset")]
        UnknownAsset,
        [EnumMember(Value = "Cancelled")]
        Cancelled,
        [EnumMember(Value = "LeadToNegativeSpread")]
        LeadToNegativeSpread
    }
    internal static class OrderStatusEnumExtension
    {
        internal static string ToSerializedValue(this OrderStatus? value)
        {
            return value == null ? null : ((OrderStatus)value).ToSerializedValue();
        }

        internal static string ToSerializedValue(this OrderStatus value)
        {
            switch( value )
            {
                case OrderStatus.Pending:
                    return "Pending";
                case OrderStatus.InOrderBook:
                    return "InOrderBook";
                case OrderStatus.Processing:
                    return "Processing";
                case OrderStatus.Matched:
                    return "Matched";
                case OrderStatus.NotEnoughFunds:
                    return "NotEnoughFunds";
                case OrderStatus.NoLiquidity:
                    return "NoLiquidity";
                case OrderStatus.UnknownAsset:
                    return "UnknownAsset";
                case OrderStatus.Cancelled:
                    return "Cancelled";
                case OrderStatus.LeadToNegativeSpread:
                    return "LeadToNegativeSpread";
            }
            return null;
        }

        internal static OrderStatus? ParseOrderStatus(this string value)
        {
            switch( value )
            {
                case "Pending":
                    return OrderStatus.Pending;
                case "InOrderBook":
                    return OrderStatus.InOrderBook;
                case "Processing":
                    return OrderStatus.Processing;
                case "Matched":
                    return OrderStatus.Matched;
                case "NotEnoughFunds":
                    return OrderStatus.NotEnoughFunds;
                case "NoLiquidity":
                    return OrderStatus.NoLiquidity;
                case "UnknownAsset":
                    return OrderStatus.UnknownAsset;
                case "Cancelled":
                    return OrderStatus.Cancelled;
                case "LeadToNegativeSpread":
                    return OrderStatus.LeadToNegativeSpread;
            }
            return null;
        }
    }
}
