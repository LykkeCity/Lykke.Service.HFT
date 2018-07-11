using System.Collections.Generic;
using JetBrains.Annotations;

namespace Lykke.Service.HFT.Contracts.Health
{
    /// <summary>
    /// Response class for IsAlive request.
    /// </summary>
    [PublicAPI]
    public class IsAliveModel
    {
        /// <summary>
        /// The name of the service.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The version of the service.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// The environment of the service (pod name).
        /// </summary>
        public string Env { get; set; }

        /// <summary>
        /// Indicating whether this instance is in debug or production mode.
        /// </summary>
        public bool IsDebug { get; set; }

        /// <summary>
        /// The possible health issues of the service.
        /// </summary>
        public IEnumerable<IssueModel> IssueIndicators { get; set; }
    }
}