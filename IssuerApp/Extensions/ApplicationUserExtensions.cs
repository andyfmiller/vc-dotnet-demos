using IssuerApp.Data.Models;
using Microsoft.AspNetCore.Identity;

namespace IssuerApp.Extensions
{
    public static class ApplicationUserExtensions
    {
        public static async Task<string> GetUserRole(this ApplicationUser user, UserManager<ApplicationUser> userManager)
        {

            if (userManager == null) return string.Empty;

            var role = string.Empty;

            foreach (var userRole in Constants.Roles.UserRoles)
            {
                if (await userManager.IsInRoleAsync(user, userRole))
                {
                    role = userRole;
                    break;
                }
            }

            return role;
        }
    }
}
