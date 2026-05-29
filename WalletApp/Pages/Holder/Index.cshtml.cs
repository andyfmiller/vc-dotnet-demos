using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WalletApp.Data;
using WalletApp.Data.Models;
using WalletApp.Pages.Shared;

namespace WalletApp.Pages.Holder
{
    [IgnoreAntiforgeryToken]
    public class IndexModel : WalletPageModel
    {
        public IndexModel(
            ApplicationDbContext context,
            ILogger<IndexModel> logger,
            UserManager<ApplicationUser> userManager)
            : base(context, logger, userManager) { }

        public List<HolderCredential> HolderCredentials { get; private set; } = [];

        public async Task<IActionResult> OnGetAsync()
        {
            if (AppUser == null) return Page();

            // For Admin role redirect to the Admin/Holders 
            if (await UserManager.IsInRoleAsync(AppUser, Constants.Roles.Admin))
            {
                return RedirectToPage("/Admin/Holders/Index");
            }

            // Ensure Holder users have a selected holder
            if (await UserManager.IsInRoleAsync(AppUser, Constants.Roles.Holder))
            {
                if (AppUser.SelectedHolder == null)
                {
                    var firstHolder = await Context.Holders
                        .OrderBy(o => o.HolderKey)
                        .FirstOrDefaultAsync();

                    if (firstHolder != null)
                    {
                        AppUser.SelectedHolder = firstHolder;
                        await UserManager.UpdateAsync(AppUser);
                    }
                    else
                    {
                        Logger.LogWarning("No holders found in the database");
                        foreach (var role in Constants.Roles.UserRoles)
                        {
                            if (await UserManager.IsInRoleAsync(AppUser, role))
                            {
                                await UserManager.RemoveFromRoleAsync(AppUser, role);
                            }
                        }

                        await UserManager.AddToRoleAsync(AppUser, Constants.Roles.Admin);
                        return Redirect(Url.Content("~/"));
                    }
                }
            }

            // Load credentials for the selected holder
            HolderCredentials = (await Context.HolderCredentials
                .Where(c => c.HolderKey == AppUser.SelectedHolderKey)
                .ToListAsync())
                .OrderByDescending(c => c.ReceivedAt)
                .ToList();

            return Page();
        }
    }
}
