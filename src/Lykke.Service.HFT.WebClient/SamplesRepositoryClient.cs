using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Http;
using Lykke.Service.HFT.Abstractions;

namespace Lykke.Service.HFT.WebClient
{
    public class SamplesRepositoryClient : ISamplesRepository
    {
        private readonly RestClient _restClient;
        private const string Endpoint = "api/samples";

        public SamplesRepositoryClient(HFTRestClientConfig config)
        {
            _restClient = new RestClient(config);
        }

        public Task InsertAsync(ISample model)
        {
            return _restClient.PostAsync($"{Endpoint}", model);
        }

        public Task UpdateAsync(ISample model)
        {
            return _restClient.PutAsync($"{Endpoint}/{model.Id}", model);
        }

        public async Task<ISample> GetAsync(string id)
        {
            return await _restClient.GetResultAsync<Sample>($"{Endpoint}/{id}");
        }

        public async Task<IEnumerable<ISample>> GetAsync()
        {
            return await _restClient.GetResultAsync<IEnumerable<Sample>>($"{Endpoint}");
        }
    }
}