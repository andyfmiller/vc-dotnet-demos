using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VerifierApp.Data;
using VerifierApp.Data.Models;

namespace VerifierApp.Pages.Shared
{
    [IgnoreAntiforgeryToken]
    public class VerifierPageModel : PageModel
    {
        protected readonly ApplicationDbContext Context;
        protected readonly ILogger Logger;
        protected readonly UserManager<ApplicationUser> UserManager;

        public VerifierPageModel(
            ApplicationDbContext context,
            ILogger logger,
            UserManager<ApplicationUser> userManager)
        {
            Context = context;
            Logger = logger;
            UserManager = userManager;
        }

        public ApplicationUser? AppUser { get; private set; }

        public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                AppUser = await UserManager.GetUserAsync(User);
                if (AppUser != null)
                {
                    ViewData["AppUser"] = AppUser;
                }
            }
            await next();
        }

        /// <summary>
        /// Handles POST from persona selector.
        /// </summary>
        /// <returns>Redirect to app home page.</returns>
        public async Task<IActionResult> OnPostChangeRole(string navRole)
        {
            if (User.Identity is not { IsAuthenticated: true }) return Page();

            if (AppUser is null) return RedirectToPage("/Logout");

            if (!string.IsNullOrEmpty(navRole))
            {
                if (!await UserManager.IsInRoleAsync(AppUser, navRole))
                {
                    foreach (var role in Constants.Roles.UserRoles)
                    {
                        if (await UserManager.IsInRoleAsync(AppUser, role))
                        {
                            await UserManager.RemoveFromRoleAsync(AppUser, role);
                        }
                    }

                    await UserManager.AddToRoleAsync(AppUser, navRole);
                    return Redirect(Url.Content("~/"));
                }
            }

            // Remove handler from URL

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
        /// Delete a requirement.
        /// </summary>
        /// <param name="key">The entity key to delete.</param>
        /// <param name="pageName">Optional redirect page on success.</param>
        public virtual async Task<IActionResult> OnPostDeleteRequirement(int? key, string? pageName = null)
        {
            var requirement = await Context.Requirements.FindAsync(key);
            if (requirement != null)
            {
                try
                {
                    Context.Attach(requirement).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
                    await Context.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Cannot delete requirement");
                    ModelState.AddModelError(string.Empty, "Cannot delete requirement");
                    return Page();
                }
            }

            if (string.IsNullOrEmpty(pageName))
            {
                return Page();
            }

            return RedirectToPage(pageName);
        }
    }
}