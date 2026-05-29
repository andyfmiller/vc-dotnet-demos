using IssuerApp.Data;
using IssuerApp.Data.Models;
using IssuerApp.Pages.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace IssuerApp.Pages.Admin.Organizations
{
    public class IndexModel : IssuerAppPageModel
    {
        public IndexModel(
            ApplicationDbContext context,
            ILogger<IndexModel> logger,
            UserManager<ApplicationUser> userManager)
            : base(context, logger, userManager) { }

        public List<OrganizationViewModel> Organizations { get; set; } = [];

        public async Task OnGetAsync(string? message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                ViewData["Message"] = message;
            }

            await LoadOrganizationsAsync();
        }

        public override async Task<IActionResult> OnPostDeleteOrganization(int? key, string? pageName = null)
        {
            var result = await base.OnPostDeleteOrganization(key, pageName);

            if (result is PageResult)
            {
                await LoadOrganizationsAsync();
            }

            return result;
        }

        private async Task LoadOrganizationsAsync()
        {
            Organizations = await Context.Organizations
                .Include(x => x.Profile)
                .ThenInclude(p => p!.Image)
                .OrderBy(x => x.Profile != null ? x.Profile.Name : null)
                .Select(x => new OrganizationViewModel
                {
                    Organization = x,
                    MemberCount = x.Members.Count,
                    AchievementCount = x.Achievements.Count,
                    CredentialCount = x.AchievementCredentials.Count
                })
                .ToListAsync();
        }
    }

    public class OrganizationViewModel
    {
        public required Organization Organization { get; set; }
        public int MemberCount { get; set; }
        public int AchievementCount { get; set; }
        public int CredentialCount { get; set; }
    }
}