using Lykke.Service.HFT.WebApi.Middleware.Validator;

namespace Lykke.Service.HFT.WebApi.Validator.Middleware
{
	public class FixedApiKeyValidator : IApiKeyValidator
    {
        public bool Validate(string apiKey)
        {
            return apiKey == "key";
        }
    }
}
