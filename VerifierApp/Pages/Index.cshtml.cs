using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VerifierApp.Data;
using VerifierApp.Data.Models;
using VerifierApp.Pages.Shared;

namespace VerifierApp.Pages
{
    [IgnoreAntiforgeryToken]
    public class IndexModel : VerifierPageModel
    {
        public IndexModel(
            ApplicationDbContext context,
            ILogger<IndexModel> logger,
            UserManager<ApplicationUser> userManager)
            : base(context, logger, userManager)
        {
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (AppUser == null) return Page();

            // For Admin role redirect to the Admin/Requirements
            if (await UserManager.IsInRoleAsync(AppUser, Constants.Roles.Admin))
            {
                return RedirectToPage("/Admin/Requirements/Index");
            }

            // Holder role redirects to the HolderPortal/Require
            if (await UserManager.IsInRoleAsync(AppUser, Constants.Roles.Holder))
            {
                return RedirectToPage("HolderPortal/Require");
            }

            return Page();
        }
    }
}
