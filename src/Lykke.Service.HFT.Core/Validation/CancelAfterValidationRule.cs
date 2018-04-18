using System;

namespace Lykke.Service.HFT.Core.Validation
{
    public static class CancelAfterValidationRule
    {
        public static bool IsValid(DateTime dateTime)
            => (dateTime - DateTime.Now).TotalSeconds > double.Epsilon;
    }
}
