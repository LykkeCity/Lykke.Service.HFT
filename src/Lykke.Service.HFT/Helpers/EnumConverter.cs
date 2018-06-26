using System;

namespace Lykke.Service.HFT.Helpers
{
    internal static class EnumConverter
    {
        public static T ConvertToEnum<T>(this object value, T defaultValue)
            where T : struct
        {
            if (value == null)
            {
                return defaultValue;
            }

            return Enum.TryParse(value.ToString(), out T result) ? result : defaultValue;
        }
    }
}