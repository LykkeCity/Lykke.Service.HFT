namespace Lykke.Service.HFT.WebApi.Middleware.Validator
{
	public interface IApiKeyValidator
    {
        bool Validate(string apiKey);
    }
}
