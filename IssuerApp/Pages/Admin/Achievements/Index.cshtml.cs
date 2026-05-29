using IssuerApp.Data;
using IssuerApp.Data.Models;
using IssuerApp.Data.Models.OpenBadges;
using IssuerApp.Pages.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IssuerApp.Pages.Admin.Achievements
{
    public class IndexModel : IssuerAppPageModel
    {
        public IndexModel(
            ApplicationDbContext context,
            ILogger<IndexModel> logger,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration)
            : base(context, logger, userManager)
        {
        }

        public List<Achievement> Achievements { get; set; } = [];
        public string SelectedOrganization { get; set; } = string.Empty;

        public async Task OnGetAsync(string? message)
        {
            ViewData["Title"] = NavPages.Achievements;
            ViewData["ActivePage"] = NavPages.Achievements;
            if (!string.IsNullOrEmpty(message))
            {
                ViewData["Message"] = message;
            }
            Achievements = await GetOrgAchievements();
            SelectedOrganization = AppUser!.SelectedOrganization?.DisplayName ?? "n/a";
        }

        public override async Task<IActionResult> OnPostDeleteAchievement(int? key, string? pageName = null)
        {
            var result = await base.OnPostDeleteAchievement(key, pageName);
            if (result is PageResult)
            {
                Achievements = await GetOrgAchievements();
            }
            return result;
        }

    }
}