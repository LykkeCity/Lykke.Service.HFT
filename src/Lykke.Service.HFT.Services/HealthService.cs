using Lykke.Service.HFT.Core.Domain.Health;
using Lykke.Service.HFT.Core.Services;
using System.Collections.Generic;

namespace Lykke.Service.HFT.Services
{
    // NOTE: See https://lykkex.atlassian.net/wiki/spaces/LKEWALLET/pages/35755585/Add+your+app+to+Monitoring
    public class HealthService : IHealthService
    {
        public string GetHealthViolationMessage()
        {
            // TODO: Check gathered health statistics, and return appropriate health violation message, or NULL if service hasn't critical errors
            return null;
        }

        public IEnumerable<HealthIssue> GetHealthIssues()
        {
            var issues = new HealthIssuesCollection
            {
                {"OrderCounter", Lykke.Service.HFT.Core.Constants.OrderCounter.ToString()},
                {"TotalProcessingTime", Lykke.Service.HFT.Core.Constants.TotalProcessingTime.ToString()},
                {"ValidationTime", Lykke.Service.HFT.Core.Constants.ValidationTime.ToString()},
                {"MongoTime", Lykke.Service.HFT.Core.Constants.MongoTime.ToString()},
                {"AssetPairTime", Lykke.Service.HFT.Core.Constants.AssetPairTime.ToString()},
                {"FeeTime", Lykke.Service.HFT.Core.Constants.FeeTime.ToString()},
                {"MeTime", Lykke.Service.HFT.Core.Constants.MeTime.ToString()},
            };
            return issues;
        }
    }
}