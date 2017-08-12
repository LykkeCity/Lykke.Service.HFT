using System;
using Lykke.Service.HFT.WebApi.Middleware.Validator;

namespace Lykke.Service.HFT.WebApi.Validator.Middleware
{
	public class ApiKeyValidator : IApiKeyValidator
    {
        public bool Validate(string apiKey)
        {
			// todo: implement api key to cliend id validation
	        throw new NotImplementedException();
        }
    }
}
