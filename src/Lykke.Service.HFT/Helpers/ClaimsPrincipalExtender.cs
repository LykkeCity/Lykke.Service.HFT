using System.Security.Claims;

namespace Lykke.Service.HFT.Helpers
{
    internal static class ClaimsPrincipalExtender
    {
        public static string GetWalletId(this ClaimsPrincipal user)
        {
            if (!user.Identity.IsAuthenticated)
                return null;

            return user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
