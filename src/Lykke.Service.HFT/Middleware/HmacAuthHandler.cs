using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Common;
using JetBrains.Annotations;
using Lykke.Service.HFT.Core.Services;
using Lykke.Service.HFT.Core.Settings;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lykke.Service.HFT.Middleware
{
    [UsedImplicitly]
    internal class HmacAuthHandler : AuthenticationHandler<HmacAuthOptions>
    {
        private readonly IHftClientService _hftClientService;
        private readonly uint _requestLifetimeInMsec;
        private string _failReason;

        public HmacAuthHandler(IOptionsMonitor<HmacAuthOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock,
            IHftClientService hftClientService,
            RequestSettings requestSettings
            ) : base(options, logger, encoder, clock)
        {
            _hftClientService = hftClientService;
            _requestLifetimeInMsec = requestSettings.LifetimeInMsec;
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = 401;

            if (_failReason != null)
            {
                Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = _failReason;
            }

            return Task.CompletedTask;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Context.Request.Headers.TryGetValue(HmacAuthOptions.DefaultHeaderName, out var authorizationHeaderValues))
                return AuthenticateResult.NoResult();

            //Authorization header: HMAC:apiKey:timestamp:signature
            string authorizationHeader = authorizationHeaderValues.First();

            if (!authorizationHeader.StartsWith(HmacAuthOptions.AuthenticationScheme))
                return Fail(HmacAuthOptions.FailReason.InvalidHeader);

            (string apiKey, double timestamp, string signature) = GetAutherizationHeaderValues(authorizationHeader);

            if (apiKey == null || signature == null || timestamp == 0)
                return Fail(HmacAuthOptions.FailReason.InvalidHeader);

            double serverTimestamp = DateTime.UtcNow.ToUnixTime();

            //TODO: better implementation for replay attack
            if (Math.Abs(serverTimestamp - timestamp) > _requestLifetimeInMsec)
                return Fail(HmacAuthOptions.FailReason.RequestExpired);

            var key = await _hftClientService.GetApiKey(apiKey);

            if (key == null)
                return Fail(HmacAuthOptions.FailReason.InvalidApiKey);

            if (string.IsNullOrEmpty(key.SecretKey))
                return Fail(HmacAuthOptions.FailReason.RegenerateKey);

            string json;

            Context.Request.EnableRewind();

            using (var reader = new StreamReader(Context.Request.Body, Encoding.UTF8, true, 1024, true))
            {
                json = await reader.ReadToEndAsync();
            }

            Context.Request.Body.Seek(0, SeekOrigin.Begin);

            if (string.IsNullOrEmpty(json))
                return Fail(HmacAuthOptions.FailReason.InvalidHeader);

            bool isValid;

            using (var hmac = new HMACSHA256(key.SecretKey.ToUtf8Bytes()))
            {
                var computedSignature = hmac.ComputeHash(json.ToUtf8Bytes()).ToHexString().ToLower();
                isValid = computedSignature.Equals(signature, StringComparison.InvariantCultureIgnoreCase);
            }

            if (isValid)
            {
                var identity = new ClaimsIdentity(HmacAuthOptions.AuthenticationScheme);
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, key.WalletId));
                var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), null, HmacAuthOptions.AuthenticationScheme);
                return AuthenticateResult.Success(ticket);
            }

            return Fail(HmacAuthOptions.FailReason.InvalidHeader);
        }

        private static (string apiKey, double timestamp, string signature) GetAutherizationHeaderValues(string authHeader)
        {
            var values = authHeader.TrimStart($"{HmacAuthOptions.AuthenticationScheme} ".ToCharArray()).Split(':');

            if (values.Length == 3 && double.TryParse(values[1], out var msec))
            {
                var requestTimestamp = (new DateTime(1970, 1, 1)).AddMilliseconds(msec).ToUnixTime();

                return (values[0], requestTimestamp, values[2]);
            }

            return (null, 0, null);
        }

        private AuthenticateResult Fail(HmacAuthOptions.FailReason reason)
        {
            _failReason = reason.ToString();
            return AuthenticateResult.Fail(_failReason);
        }
    }
}
