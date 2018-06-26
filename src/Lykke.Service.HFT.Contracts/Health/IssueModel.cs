using JetBrains.Annotations;

namespace Lykke.Service.HFT.Contracts.Health
{
    /// <summary>
    /// Issue description.
    /// </summary>
    [PublicAPI]
    public class IssueModel
    {
        /// <summary>
        /// Gets or sets the issue type.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the issue reason, message or value.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Creates a new health issue model.
        /// </summary>
        /// <param name="type">The issue type.</param>
        /// <param name="value">The issue value.</param>
        public static IssueModel Create(string type, string value)
        {
            return new IssueModel
            {
                Type = type,
                Value = value
            };
        }
    }
}