using IssuerApp.Data;
using IssuerApp.Data.Models;
using IssuerApp.Pages.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IssuerApp.Pages
{
    [IgnoreAntiforgeryToken]
    public class IndexModel : IssuerAppPageModel
    {
        public IndexModel(
            ApplicationDbContext context,
            ILogger<IndexModel> logger,
            UserManager<ApplicationUser> userManager)
            : base(context, logger, userManager) { }

        public async Task<IActionResult> OnGetAsync()
        {
            if (AppUser == null) return Page();

            if (User.Identity == null || !User.Identity.IsAuthenticated) return Page();

            // Ensure Admin users have a selected organization
            if (await UserManager.IsInRoleAsync(AppUser, Constants.Roles.Admin) && AppUser.SelectedOrganization == null)
            {
                var firstOrganization = await Context.Organizations
                    .OrderBy(o => o.OrganizationKey)
                    .FirstOrDefaultAsync();

                if (firstOrganization != null)
                {
                    AppUser.SelectedOrganization = firstOrganization;
                    await UserManager.UpdateAsync(AppUser);
                }
            }

            // For Holder role, ensure a member is selected then redirect to the Holder dashboard
            if (await UserManager.IsInRoleAsync(AppUser, Constants.Roles.Holder))
            {
                if (AppUser.SelectedMemberKey == null)
                {
                    var firstMember = await Context.Members
                        .OrderBy(r => r.MemberKey)
                        .FirstOrDefaultAsync();

                    if (firstMember != null)
                    {
                        AppUser.SelectedMember = firstMember;
                        await UserManager.UpdateAsync(AppUser);
                    }
                }

                return RedirectToPage("/HolderPortal/Index");
            }

            return Page();
        }
    }
}
