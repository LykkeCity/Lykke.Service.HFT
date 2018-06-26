using System;
using System.Net;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.HFT.Contracts;
using Refit;

namespace Lykke.Service.HFT.Client
{
    /// <summary>
    /// Helper methods to work with the refit clients.
    /// </summary>
    [PublicAPI]
    public static class ClientExtensions
    {
        /// <summary>
        /// Try to execute the refit api client task and return the response on success or the error on failure.
        /// </summary>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="task">The refit task to execute.</param>
        /// <returns>The http status code and parsed response or the error</returns>
        public static async Task<RestClientResult<TResponse>> TryExecute<TResponse>(this Task<TResponse> task)
        {
            try
            {
                var response = await task;
                return new RestClientResult<TResponse>(HttpStatusCode.OK, ResponseModel<TResponse>.CreateOk(response));
            }
            catch (ApiException apiException)
            {
                var response = HandleApiException(apiException, ResponseModel<TResponse>.CreateFail);
                return new RestClientResult<TResponse>(apiException.StatusCode, response);
            }
        }

        /// <summary>
        /// Try to execute the refit api client task and return the possible error on failure.
        /// </summary>
        /// <param name="task">The refit task to execute.</param>
        /// <returns>The http status code and possible error</returns>
        public static async Task<RestClientResult> TryExecute(this Task task)
        {
            try
            {
                await task;
                return new RestClientResult(HttpStatusCode.OK, ResponseModel.CreateOk());
            }
            catch (ApiException apiException)
            {
                var response = HandleApiException(apiException, ResponseModel.CreateFail);
                return new RestClientResult(apiException.StatusCode, response);
            }
        }

        private static T HandleApiException<T>(ApiException apiException, Func<ErrorModel, T> createResponse)
            where T : ResponseModel
        {
            var error = new ErrorModel
            {
                Code = ErrorCodeType.Runtime,
                Message = apiException.ReasonPhrase
            };

            if (apiException.HasContent)
            {
                try
                {
                    error = apiException.GetContentAs<ErrorModel>();
                }
                catch
                {
                    // Not an error model check if its already a response model.
                    try
                    {
                        var response = apiException.GetContentAs<T>();
                        if (response != null)
                        {
                            return response;
                        }
                    }
                    catch
                    {
                        // Ignore
                    }
                }
            }

            return createResponse(error);
        }
    }
}