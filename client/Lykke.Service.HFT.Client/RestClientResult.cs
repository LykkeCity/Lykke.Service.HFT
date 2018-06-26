using System;
using System.Net;
using JetBrains.Annotations;
using Lykke.Service.HFT.Contracts;

namespace Lykke.Service.HFT.Client
{
    /// <summary>
    /// Wrapper class for a rest client response and the http status code.
    /// </summary>
    [PublicAPI]
    public class RestClientResult
    {
        private readonly ResponseModel _response;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestClientResult"/> class.
        /// </summary>
        public RestClientResult(HttpStatusCode statusCode, ResponseModel response)
        {
            _response = response ?? throw new ArgumentNullException(nameof(response));
            StatusCode = statusCode;
        }

        /// <summary>
        /// The http status code.
        /// </summary>
        public HttpStatusCode StatusCode { get; }

        /// <summary>
        /// The rest client error.
        /// </summary>
        [CanBeNull]
        public ErrorModel Error => _response.Error;

        /// <summary>
        /// Indicating whether this rest client call was a success.
        /// </summary>
        public bool Success => (int)StatusCode >= 200 && (int)StatusCode < 300;
    }

    /// <summary>
    /// Wrapper class for a rest client response and the http status code.
    /// </summary>
    [PublicAPI]
    public class RestClientResult<T> : RestClientResult
    {
        private readonly ResponseModel<T> _response;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestClientResult{T}"/> class.
        /// </summary>
        public RestClientResult(HttpStatusCode statusCode, ResponseModel<T> response)
            : base(statusCode, response)
        {
            _response = response ?? throw new ArgumentNullException(nameof(response));
        }

        /// <summary>
        /// The rest client result.
        /// </summary>
        public T Result => _response.Result;
    }
}