using System.Collections.Generic;
using Lykke.Service.HFT.Contracts.Health;
using Lykke.Service.HFT.Core.Services;

namespace Lykke.Service.HFT.Services
{
    // NOTE: See https://lykkex.atlassian.net/wiki/spaces/LKEWALLET/pages/35755585/Add+your+app+to+Monitoring
    public class HealthService : IHealthService
    {
        public string GetHealthViolationMessage() => null;

        public IEnumerable<IssueModel> GetHealthIssues() => new IssueModel[0];
    }
}