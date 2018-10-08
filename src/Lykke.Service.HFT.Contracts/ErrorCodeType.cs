namespace Lykke.Service.HFT.Contracts
{
    /// <summary>
    /// Well known error codes enumeration
    /// </summary>
    public enum ErrorCodeType
    {
        /// <summary>
        /// One of the input parameters was invalid.
        /// </summary>
        InvalidInputField = 0,

        /// <summary>
        /// The order was rejected.
        /// </summary>
        Rejected = 1,

        /// <summary>
        /// Runtime error
        /// </summary>
        Runtime = 500,
    }
}