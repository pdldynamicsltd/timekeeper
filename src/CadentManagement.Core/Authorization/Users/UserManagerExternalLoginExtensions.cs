using System.Linq;
using System.Threading.Tasks;
using CadentManagement.Authorization;

namespace CadentManagement.Authorization.Users;

public static class UserManagerExternalLoginExtensions
{
    public static async Task<bool> IsExternalLoginOnlyAsync(this UserManager userManager, User user)
    {
        var claims = await userManager.GetClaimsAsync(user);
        return claims.Any(c => c.Type == ExternalLoginConsts.ExternalLoginOnlyClaimType);
    }

    public static async Task ClearExternalLoginOnlyClaimAsync(this UserManager userManager, User user)
    {
        var claims = await userManager.GetClaimsAsync(user);
        var matchingClaims = claims.Where(c => c.Type == ExternalLoginConsts.ExternalLoginOnlyClaimType).ToList();
        foreach (var claim in matchingClaims)
        {
            await userManager.RemoveClaimAsync(user, claim);
        }
    }
}
