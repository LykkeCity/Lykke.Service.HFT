namespace Lykke.Service.HFT.Contracts
{
    /// <summary>
    /// Model class for error responses
    /// </summary>
    public class ErrorModel
    {
        /// <summary>
        /// Well-known error code.
        /// </summary>
        public ErrorCodeType Code { get; set; } = ErrorCodeType.Runtime;

        /// <summary>
        /// In case ErrorCoderType = 0
        /// </summary>
        public string Field { get; set; }

        /// <summary>
        /// Localized Error message
        /// </summary>
        public string Message { get; set; }
    }
}