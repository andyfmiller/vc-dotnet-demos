using IssuerApp.Data;
using IssuerApp.Data.Models;
using IssuerApp.Data.Models.OpenBadges;
using IssuerApp.Pages.Shared;
using Library.Models.OpenBadges;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IssuerApp.Pages.Admin.AchievementCredentials
{
    public class AchievementCredentialsPageModel : IssuerAppPageModel
    {
        protected AchievementCredentialsPageModel(
            ApplicationDbContext context,
            ILogger logger,
            UserManager<ApplicationUser> userManager)
            : base(context, logger, userManager) { }

        [BindProperty]
        public required Data.Models.OpenBadges.AchievementCredential AchievementCredential { get; set; }

        /// <summary>
        /// Returns a <see cref="List{SelectListItem}"/> with
        /// value = <see cref="Achievement.AchievementKey"/>
        /// </summary>
        public async Task<List<SelectListItem>> GetAchievementSelectList()
        {
            return (await GetOrgAchievements())
                .Select(x => new SelectListItem(x.DisplayName, x.AchievementKey.ToString()))
                .ToList();
        }

        /// <summary>
        /// Returns a <see cref="List{SelectListItem}"/> with
        /// value = <see cref="Organization.OrganizationKey"/>
        /// </summary>
        public async Task<List<SelectListItem>> GetOrganizationSelectList()
        {
            return await Context.Organizations
                .Select(x => new SelectListItem(x.Profile != null ? x.Profile.Name : null, x.OrganizationKey.ToString()))
                .ToListAsync();
        }

        protected async Task<bool> TryValidateModel()
        {
            var achievement = AchievementCredential.CredentialSubject?.Achievement as Data.Models.OpenBadges.Achievement;
            if (achievement?.AchievementKey == null)
            {
                ModelState.AddModelError(
                    "AchievementCredential.CredentialSubject.Achievement",
                    "The Achievement field is required.");
            }

            return ModelState.IsValid;
        }
    }
}
