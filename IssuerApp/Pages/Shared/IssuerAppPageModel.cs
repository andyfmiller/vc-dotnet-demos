using IssuerApp.Data;
using IssuerApp.Data.Models;
using IssuerApp.Data.Models.OpenBadges;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace IssuerApp.Pages.Shared
{
    [IgnoreAntiforgeryToken]
    public class IssuerAppPageModel : PageModel
    {
        protected readonly ApplicationDbContext Context;
        protected readonly ILogger Logger;
        protected readonly UserManager<ApplicationUser> UserManager;

        public ApplicationUser? AppUser { get; private set; }

        public IssuerAppPageModel(
            ApplicationDbContext context,
            ILogger logger,
            UserManager<ApplicationUser> userManager)
        {
            Context = context;
            Logger = logger;
            UserManager = userManager;
        }

        public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                AppUser = await UserManager.GetUserAsync(User);
                if (AppUser != null)
                {
                    await Context.Entry(AppUser).Reference(u => u.SelectedOrganization).LoadAsync();
                    await Context.Entry(AppUser).Reference(u => u.SelectedMember).LoadAsync();
                    ViewData["AppUser"] = AppUser;
                }
            }
            await next();
        }

        /// <summary>
        /// Handles POST from persona selector.
        /// </summary>
        /// <returns>Redirect to app home page.</returns>
        public async Task<IActionResult> OnPostChangeOrg(int? navOrgKey)
        {
            if (User.Identity is not { IsAuthenticated: true }) return Page();

            if (navOrgKey.HasValue)
            {
                if (AppUser!.SelectedOrganizationKey != navOrgKey)
                {
                    var organization = await Context.Organizations.FindAsync(navOrgKey);
                    if (organization is not null)
                    {
                        AppUser.SelectedOrganization = organization;
                        await UserManager.UpdateAsync(AppUser);
                    }
                }
            }

            return RedirectWithoutHandler();
        }

        /// <summary>
        /// Handles POST from member selector.
        /// </summary>
        /// <returns>Redirect to current page without handler.</returns>
        public async Task<IActionResult> OnPostChangeMember(int? navMemberKey)
        {
            if (User.Identity is not { IsAuthenticated: true }) return Page();

            if (navMemberKey.HasValue)
            {
                if (AppUser!.SelectedMemberKey != navMemberKey)
                {
                    var member = await Context.Members.FindAsync(navMemberKey);
                    if (member is not null)
                    {
                        AppUser.SelectedMember = member;
                        await UserManager.UpdateAsync(AppUser);

                        return Redirect(Url.Content("~/MemberPortal/Index"));
                    }
                }
            }

            return RedirectWithoutHandler();
        }

        /// <summary>
        /// Handles POST from persona selector.
        /// </summary>
        /// <returns>Redirect to app home page.</returns>
        public async Task<IActionResult> OnPostChangeRole(string navRole)
        {
            if (User.Identity is not { IsAuthenticated: true }) return Page();

            if (!string.IsNullOrEmpty(navRole))
            {
                if (!await UserManager.IsInRoleAsync(AppUser!, navRole))
                {
                    foreach (var role in Constants.Roles.UserRoles)
                    {
                        if (await UserManager.IsInRoleAsync(AppUser!, role))
                        {
                            await UserManager.RemoveFromRoleAsync(AppUser!, role);
                        }
                    }

                    await UserManager.AddToRoleAsync(AppUser!, navRole);

                    // Redirect back to the home page when role is changed

                    return Redirect(Url.Content("~/"));
                }
            }

            return RedirectWithoutHandler();
        }

        private IActionResult RedirectWithoutHandler()
        {
            var path = $"{Request.PathBase}{Request.Path}";
            var query = new QueryString();
            foreach (var parameter in Request.Query)
            {
                if (!parameter.Key.Equals("handler", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Add(parameter.Key, parameter.Value.ToString());
                }
            }
            return Redirect($"{path}{query.Value}");
        }

        /// <summary>
        /// Delete an achievement.
        /// </summary>
        /// <param name="key">The entity key to delete.</param>
        /// <param name="pageName">Optional redirect page on success.</param>
        public virtual async Task<IActionResult> OnPostDeleteAchievement(int? key, string? pageName = null)
        {
            var achievement = await Context.Achievements
                .SingleOrDefaultAsync(x => x.AchievementKey == key);

            if (achievement != null)
            {
                // Finally delete the achievement

                Context.Attach(achievement).State = EntityState.Deleted;

                try
                {
                    await Context.SaveChangesAsync();
                    Logger.LogInformation($"Deleted achievement: {key}");
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Cannot delete achievement");
                    ModelState.AddModelError(string.Empty, "Cannot delete achievement");
                    return Page();
                }
            }

            if (string.IsNullOrEmpty(pageName))
            {
                return Page();
            }

            return RedirectToPage(pageName);
        }

        /// <summary>
        /// Delete an AchievementCredential.
        /// </summary>
        /// <param name="key">The entity key to delete.</param>
        /// <param name="pageName">Optional redirect page on success.</param>
        public virtual async Task<IActionResult> OnPostDeleteAchievementCredential(int? key, string? pageName = null)
        {
            var achievementCredential = await Context.AchievementCredentials
                .SingleOrDefaultAsync(x => x.AchievementCredentialKey == key);

            if (achievementCredential != null)
            {
                Context.Attach(achievementCredential).State = EntityState.Deleted;

                try
                {
                    await Context.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Cannot delete Open Badge");
                    ModelState.AddModelError(string.Empty, "Cannot delete Open Badge");
                    return Page();
                }
            }

            if (string.IsNullOrEmpty(pageName))
            {
                return Page();
            }

            return RedirectToPage(pageName);
        }

        /// <summary>
        /// Delete an organization.
        /// </summary>
        /// <param name="key">The entity key to delete.</param>
        /// <param name="pageName">Optional redirect page on success.</param>
        public virtual async Task<IActionResult> OnPostDeleteOrganization(int? key, string? pageName = null)
        {
            var organization = await Context.Organizations
                .SingleOrDefaultAsync(x => x.OrganizationKey == key);

            if (organization != null)
            {
                if (AppUser!.SelectedOrganization?.OrganizationKey == key)
                {
                    AppUser.SelectedOrganization = await Context.Organizations
                        .Where(x => x.OrganizationKey != key)
                        .OrderBy(x => x.Profile != null ? x.Profile.Name : null)
                        .FirstOrDefaultAsync();

                    await UserManager.UpdateAsync(AppUser);
                }

                try
                {
                    Context.Attach(organization).State = EntityState.Deleted;

                    await Context.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Cannot delete organization");
                    ModelState.AddModelError(string.Empty, "Cannot delete organization");
                    return Page();
                }
            }

            if (string.IsNullOrEmpty(pageName))
            {
                return Page();
            }

            return RedirectToPage(pageName);
        }

        public Achievement CreateAchievement()
        {
            return new Achievement
            {
                Id = $"urn:uuid:{Guid.NewGuid():D}",
                Type = ["Achievement"],
                Criteria = new Criteria
                {
                    Narrative = string.Empty
                },
                Name = string.Empty,
                Description = string.Empty,
                Organization = AppUser!.SelectedOrganization!,
                OrganizationKey = AppUser.SelectedOrganizationKey!.Value
            };
        }

        /// <summary>
        /// Create an issuer and the associated profile.
        /// </summary>
        public Organization CreateOrganization(string name, string? iconUrl = null)
        {
            var profile = new Profile
            {
                Id = $"urn:uuid:{Guid.NewGuid():D}",
                Name = name,
                Image = !string.IsNullOrEmpty(iconUrl) ? new Image { Id = iconUrl, Type = "Image" } : null
            };

            var organization = new Organization
            {
                Profile = profile,
                Members = []
            };

            return organization;
        }

        /// <summary>
        /// Create member
        /// </summary>
        public Member CreateMember(string name)
        {
            return new Member
            {
                Name = name
            };
        }

        public async Task<List<Achievement>> GetOrgAchievements()
        {
            if (AppUser?.SelectedOrganizationKey == null)
            {
                return new List<Achievement>();
            }

            var organization = await Context.Organizations
                .Include(o => o.Achievements)
                .FirstOrDefaultAsync(o => o.OrganizationKey == AppUser.SelectedOrganizationKey);

            var achievements = organization?.Achievements
                .OrderBy(x => x.HumanCode)
                .ThenBy(x => x.Name)
                .ToList() ?? new List<Achievement>();

            return achievements;
        }

        public async Task<List<AchievementCredential>> GetOrgAchievementCredentials()
        {
            if (AppUser?.SelectedOrganizationKey == null)
                return new List<AchievementCredential>();

            var organization = await Context.Organizations
                .Include(o => o.AchievementCredentials)
                    .ThenInclude(ac => ac.CredentialSubject)
                        .ThenInclude(cs => cs!.Achievement)
                .Include(o => o.AchievementCredentials)
                    .ThenInclude(ac => ac.CredentialSubject)
                        .ThenInclude(cs => cs!.Member)
                .FirstOrDefaultAsync(o => o.OrganizationKey == AppUser.SelectedOrganizationKey);

            return organization?.AchievementCredentials
                ?? new List<AchievementCredential>();
        }

        public async Task<List<Member>> GetOrgMembers()
        {
            if (AppUser?.SelectedOrganizationKey == null)
                return new List<Member>();

            var organization = await Context.Organizations
                .Include(o => o.Members)
                .FirstOrDefaultAsync(o => o.OrganizationKey == AppUser.SelectedOrganizationKey);

            return organization?.Members ?? new List<Member>();
        }
    }
}
