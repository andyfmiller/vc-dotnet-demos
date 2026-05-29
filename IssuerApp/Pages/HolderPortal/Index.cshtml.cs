using IssuerApp.Data;
using IssuerApp.Data.Models;
using IssuerApp.Data.Models.OpenBadges;
using IssuerApp.Pages.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IssuerApp.Pages.HolderPortal
{
    public class IndexModel : IssuerAppPageModel
    {
        public IndexModel(
            ApplicationDbContext context,
            ILogger<IndexModel> logger,
            UserManager<ApplicationUser> userManager)
            : base(context, logger, userManager) { }

        public Member? SelectedMember { get; private set; }

        public List<AchievementCredential> AchievementCredentials { get; private set; } = [];

        public async Task<IActionResult> OnGetAsync()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
                return RedirectToPage("/Login");

            if (AppUser == null) return RedirectToPage("/Logout");

            // Ensure the user is in the Holder role
            if (!await UserManager.IsInRoleAsync(AppUser, Constants.Roles.Holder))
                return RedirectToPage("/Index");

            // Auto-select the first member if none is selected
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

            if (AppUser.SelectedMemberKey == null)
            {
                AchievementCredentials = new List<AchievementCredential>();
                return Page();
            }

            SelectedMember = await Context.Members
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.MemberKey == AppUser.SelectedMemberKey);

            AchievementCredentials = await Context.AchievementCredentials
                .Include(ac => ac.CredentialSubject)
                    .ThenInclude(cs => cs!.Achievement)
                .Include(ac => ac.CredentialSubject)
                    .ThenInclude(cs => cs!.Member)
                .Include(ac => ac.Organization)
                .Where(ac => ac.CredentialSubject != null
                             && ac.CredentialSubject.MemberKey == AppUser.SelectedMemberKey)
                .AsNoTracking()
                .ToListAsync();

            return Page();
        }
    }
}
