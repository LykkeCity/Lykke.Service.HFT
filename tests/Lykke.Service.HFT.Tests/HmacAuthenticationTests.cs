using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Common;
using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Core.Services;
using Lykke.Service.HFT.Core.Settings;
using Lykke.Service.HFT.Middleware;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Lykke.Service.HFT.Tests
{
    public class HmacAuthenticationTests : IClassFixture<CustomWebApplicationFactory<TestStartup>>
    {
        private readonly HttpClient _client;
        private readonly FormUrlEncodedContent _content;
        private readonly string _data;
        private const string SecretKey = "secret";
        private const string ApiKeyWithoutSecretKey = "06f026d0-fdf2-43af-b6ab-2bf54ac473a8";
        private const string ApiKey = "7fd2ec58-ed47-4c39-b981-3d0033ae7b83";

        public HmacAuthenticationTests(CustomWebApplicationFactory<TestStartup> factory)
        {
            _client = factory.WithWebHostBuilder(builder =>
            {
                builder.UseSolutionRelativeContentRoot("tests/Lykke.Service.HFT.Tests");
                builder.ConfigureTestServices(services =>
                {
                    services.AddMvc().AddApplicationPart(typeof(Startup).Assembly);

                    var hftClient = new Mock<IHftClientService>();

                    hftClient.Setup(x => x.GetApiKey(ApiKey)).ReturnsAsync(() =>
                        new ApiKey {Id = Guid.Parse(ApiKey), ClientId = "test-api", SecretKey = SecretKey, WalletId = "123"});

                    hftClient.Setup(x => x.GetApiKey(ApiKeyWithoutSecretKey)).ReturnsAsync(() =>
                        new ApiKey {Id = Guid.Empty, ClientId = "test-api-old", WalletId = "111"});

                    services.AddSingleton(hftClient.Object);
                    services.AddSingleton(new RequestSettings());
                });
            }).CreateClient();

           _client.BaseAddress = new Uri("http://localhost:5000/api/v2/hmactest");

           _content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
           {
               new KeyValuePair<string, string>("Symbol", "BTCUSD"),
               new KeyValuePair<string, string>("Side", "buy"),
               new KeyValuePair<string, string>("Type", "limit"),
           });

           _data = "Symbol=BTCUSD&Side=buy&Type=limit";
        }

        [Fact]
        public async Task IsAuthenticationSuccessful()
        {
            string signature = GetSignature(_data);
            _client.DefaultRequestHeaders.Add("Authorization", $"HMAC {ApiKey}:{DateTime.UtcNow.ToUnixTime()}:{signature}");
            var response = await _client.PostAsync("", _content);

            Assert.True(response.StatusCode == HttpStatusCode.OK);

        }

        [Fact]
        public async Task IsInvalidHeader_NoHMAC()
        {
            string signature = GetSignature(_data);
            _client.DefaultRequestHeaders.Add("Authorization", $"Test {ApiKey}:{DateTime.UtcNow.ToUnixTime()}:{signature}");
            var response = await _client.PostAsync("", _content);

            Assert.True(response.StatusCode == HttpStatusCode.Unauthorized);
            Assert.Equal(response.ReasonPhrase, HmacAuthOptions.FailReason.InvalidHeader.ToString());
        }

        [Fact]
        public async Task IsInvalidHeader_BadTimeStamp()
        {
            string signature = GetSignature(_data);
            _client.DefaultRequestHeaders.Add("Authorization", $"HMAC {ApiKey}:badtimestamp:{signature}");
            var response = await _client.PostAsync("", _content);

            Assert.True(response.StatusCode == HttpStatusCode.Unauthorized);
            Assert.Equal(response.ReasonPhrase, HmacAuthOptions.FailReason.InvalidHeader.ToString());
        }

        [Fact]
        public async Task IsInvalidHeader_WrongSignature()
        {
            _client.DefaultRequestHeaders.Add("Authorization", $"HMAC {ApiKey}:{DateTime.UtcNow.ToUnixTime()}:wrongsignature");
            var response = await _client.PostAsync("", _content);

            Assert.True(response.StatusCode == HttpStatusCode.Unauthorized);
            Assert.Equal(response.ReasonPhrase, HmacAuthOptions.FailReason.InvalidHeader.ToString());
        }

        [Fact]
        public async Task IsInvalidHeader_WrongHeaderValue()
        {
            _client.DefaultRequestHeaders.Add("Authorization", $"HMAC somevalue");
            var response = await _client.PostAsync("", _content);

            Assert.True(response.StatusCode == HttpStatusCode.Unauthorized);
            Assert.Equal(response.ReasonPhrase, HmacAuthOptions.FailReason.InvalidHeader.ToString());
        }

        [Fact]
        public async Task IsRequestExpired_ReplayAttack()
        {
            string signature = GetSignature(_data);

            //set timestamp > RequestLifeTime
            _client.DefaultRequestHeaders.Add("Authorization", $"HMAC {ApiKey}:{DateTime.UtcNow.AddSeconds(-4).ToUnixTime()}:{signature}");
            var response = await _client.PostAsync("", _content);

            Assert.True(response.StatusCode == HttpStatusCode.Unauthorized);
            Assert.Equal(response.ReasonPhrase, HmacAuthOptions.FailReason.RequestExpired.ToString());
        }

        [Fact]
        public async Task IsInvalidApiKey()
        {
            string signature = GetSignature(_data);
            _client.DefaultRequestHeaders.Add("Authorization", $"HMAC nonexistingapikey:{DateTime.UtcNow.ToUnixTime()}:{signature}");
            var response = await _client.PostAsync("", _content);

            Assert.True(response.StatusCode == HttpStatusCode.Unauthorized);
            Assert.Equal(response.ReasonPhrase, HmacAuthOptions.FailReason.InvalidApiKey.ToString());
        }

        [Fact]
        public async Task IsRegenerateApiKey_ForApiKeyWithoutSecretKey()
        {
            string signature = GetSignature(_data);

            //set timestamp > RequestLifeTime
            _client.DefaultRequestHeaders.Add("Authorization", $"HMAC {ApiKeyWithoutSecretKey}:{DateTime.UtcNow.ToUnixTime()}:{signature}");
            var response = await _client.PostAsync("", _content);

            Assert.True(response.StatusCode == HttpStatusCode.Unauthorized);
            Assert.Equal(response.ReasonPhrase, HmacAuthOptions.FailReason.RegenerateKey.ToString());
        }

        private string GetSignature(string data)
        {
            string result = string.Empty;

            using (var hmac = new HMACSHA256(SecretKey.ToUtf8Bytes()))
            {
                result = hmac.ComputeHash(data.ToUtf8Bytes()).ToHexString().ToLower();
            }

            return result;
        }
    }

}
