using IssuerApp.Data;
using IssuerApp.Data.Models;
using IssuerApp.Data.Models.OpenBadges;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace IssuerApp.Pages.Admin.AchievementCredentials
{
    public class IndexModel : AchievementCredentialsPageModel
    {
        public IndexModel(
            ApplicationDbContext context,
            ILogger<IndexModel> logger,
            UserManager<ApplicationUser> userManager)
            : base(context, logger, userManager) { }

        public List<AchievementCredential> AchievementCredentials { get; set; } = [];
        public string SelectedOrganization { get; set; } = string.Empty;

        /// <summary>
        /// Used during import
        /// </summary>
        [BindProperty, Display(Name = "member")]
        public int? MemberKey { get; set; }

        public async Task OnGetAsync(string? message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                ViewData["Message"] = message;
            }
            AchievementCredentials = await GetOrgAchievementCredentials();
            SelectedOrganization = AppUser!.SelectedOrganization?.DisplayName ?? "n/a";
        }

        public override async Task<IActionResult> OnPostDeleteAchievementCredential(int? key, string? pageName = null)
        {
            var result = await base.OnPostDeleteAchievementCredential(key, pageName);
            if (result is PageResult)
            {
                AchievementCredentials = await GetOrgAchievementCredentials();
            }
            return result;
        }

    }
}