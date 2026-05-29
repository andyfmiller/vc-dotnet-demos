using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WalletApp.Data;
using WalletApp.Data.Models;
using WalletApp.Pages.Shared;

namespace WalletApp.Pages
{
    [IgnoreAntiforgeryToken]
    public class IndexModel : WalletPageModel
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

            // For Admin role redirect to the Admin/Holders 
            if (await UserManager.IsInRoleAsync(AppUser, Constants.Roles.Admin))
            {
                return RedirectToPage("/Admin/Holders/Index");
            }

            // For Holder role redirect to the Holder/Index
            if (await UserManager.IsInRoleAsync(AppUser, Constants.Roles.Holder))
            {
                return RedirectToPage("/Holder/Index");
            }

            return Page();
        }
    }
}
