using System;
using System.ComponentModel;
using JetBrains.Annotations;

namespace Lykke.Service.HFT.Core
{
    public static class Guard
    {
        /// <summary>
        ///     Ensure the <paramref name="param" /> is not null and throws <see cref="ArgumentNullException" /> if null.
        /// </summary>
        /// <exception>
        ///     Throws <see cref="ArgumentNullException" /> when a <paramref name="param" /> is null.
        /// </exception>
        /// <param name="param">
        ///     The object to validate.
        /// </param>
        /// <param name="paramName">
        ///     The name of the validating parameter.
        /// </param>
        [ContractAnnotation(@"param:null => halt")]
        public static void AgainstNull(object param, [Localizable(false)] string paramName)
        {
            if (param == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }
    }
}