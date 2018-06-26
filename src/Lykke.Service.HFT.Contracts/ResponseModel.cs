using JetBrains.Annotations;

namespace Lykke.Service.HFT.Contracts
{
    /// <summary>
    /// Response model
    /// </summary>
    public class ResponseModel
    {
        private static readonly ResponseModel OkInstance = new ResponseModel();

        /// <summary>
        /// The  possible error.
        /// </summary>
        [CanBeNull]
        public ErrorModel Error { get; set; }

        /// <summary>
        /// Creates an invalid field error response.
        /// </summary>
        /// <param name="field">The field\parameter that was invalid.</param>
        /// <param name="message">The error message.</param>
        public static ResponseModel CreateInvalidFieldError(string field, string message)
        {
            return new ResponseModel
            {
                Error = new ErrorModel
                {
                    Code = ErrorCodeType.InvalidInputField,
                    Field = field,
                    Message = message
                }
            };
        }

        /// <summary>
        /// Creates an fail response.
        /// </summary>
        /// <param name="errorCodeType">Type of the error code.</param>
        /// <param name="message">The error message.</param>
        public static ResponseModel CreateFail(ErrorCodeType errorCodeType, string message)
        {
            return new ResponseModel
            {
                Error = new ErrorModel
                {
                    Code = errorCodeType,
                    Message = message
                }
            };
        }

        /// <summary>
        /// Creates an fail response.
        /// </summary>
        /// <param name="error">The error</param>
        public static ResponseModel CreateFail(ErrorModel error)
        {
            return new ResponseModel
            {
                Error = error
            };
        }

        /// <summary>
        /// Create an ok response.
        /// </summary>
        public static ResponseModel CreateOk() => OkInstance;
    }

    /// <summary>
    /// Response model with a result
    /// </summary>
    /// <typeparam name="T">The result type</typeparam>
    public class ResponseModel<T> : ResponseModel
    {
        /// <summary>
        /// The response result.
        /// </summary>
        [CanBeNull]
        public T Result { get; set; }

        /// <summary>
        /// Create an ok response.
        /// </summary>
        /// <param name="result">the response result</param>
        public static ResponseModel<T> CreateOk(T result)
        {
            return new ResponseModel<T>
            {
                Result = result
            };
        }

        /// <summary>
        /// Creates an invalid field error response.
        /// </summary>
        /// <param name="field">The field\parameter that was invalid.</param>
        /// <param name="message">The error message.</param>
        public new static ResponseModel<T> CreateInvalidFieldError(string field, string message)
        {
            return new ResponseModel<T>
            {
                Error = new ErrorModel
                {
                    Code = ErrorCodeType.InvalidInputField,
                    Field = field,
                    Message = message
                }
            };
        }

        /// <summary>
        /// Creates an fail response.
        /// </summary>
        /// <param name="errorCodeType">Type of the error code.</param>
        /// <param name="message">The error message.</param>
        public new static ResponseModel<T> CreateFail(ErrorCodeType errorCodeType, string message)
        {
            return new ResponseModel<T>
            {
                Error = new ErrorModel
                {
                    Code = errorCodeType,
                    Message = message
                }
            };
        }

        /// <summary>
        /// Creates an fail response.
        /// </summary>
        /// <param name="error">The error</param>
        public new static ResponseModel<T> CreateFail(ErrorModel error)
        {
            return new ResponseModel<T>
            {
                Error = error
            };
        }
    }
}
