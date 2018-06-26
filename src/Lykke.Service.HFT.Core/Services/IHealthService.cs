using System.Collections.Generic;
using Lykke.Service.HFT.Contracts.Health;

namespace Lykke.Service.HFT.Core.Services
{
    // NOTE: See https://lykkex.atlassian.net/wiki/spaces/LKEWALLET/pages/35755585/Add+your+app+to+Monitoring
    public interface IHealthService
    {
        string GetHealthViolationMessage();
        IEnumerable<IssueModel> GetHealthIssues();
    }
}