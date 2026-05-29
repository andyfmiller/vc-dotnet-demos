using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WalletApp.Data;
using WalletApp.Data.Models;

namespace WalletApp.Pages.Shared
{
    [IgnoreAntiforgeryToken]
    public class WalletPageModel : PageModel
    {
        protected readonly ApplicationDbContext Context;
        protected readonly ILogger Logger;
        protected readonly UserManager<ApplicationUser> UserManager;

        public WalletPageModel(
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
                    await Context.Entry(AppUser).Reference(u => u.SelectedHolder).LoadAsync();
                    ViewData["AppUser"] = AppUser;
                }
            }
            await next();
        }

        /// <summary>
        /// Change the selected Holder.
        /// </summary>
        /// <returns>Redirect to app home page.</returns>
        public async Task<IActionResult> OnPostChangeHolder(int? holderKey)
        {
            if (User.Identity is not { IsAuthenticated: true }) return Page();

            if (AppUser is null) return RedirectToPage("/Logout");

            if (holderKey.HasValue)
            {
                if (AppUser.SelectedHolderKey != holderKey)
                {
                    var holder = await Context.Holders.FindAsync(holderKey);
                    if (holder is not null)
                    {
                        AppUser.SelectedHolder = holder;
                        await Context.SaveChangesAsync();
                        return Redirect(Url.Content("~/"));
                    }
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
        /// Delete a holder.
        /// </summary>
        /// <param name="key">The entity key to delete.</param>
        /// <param name="pageName">Optional redirect page on success.</param>
        public virtual async Task<IActionResult> OnPostDeleteHolder(int? key, string? pageName = null)
        {
            var holder = await Context.Holders.FindAsync(key);

            if (holder != null)
            {
                if (AppUser?.SelectedHolderKey == key)
                {
                    AppUser!.SelectedHolder = Context.Holders
                        .Where(x => x.HolderKey != key)
                        .OrderBy(x => x.Name)
                        .FirstOrDefault();

                    await UserManager.UpdateAsync(AppUser);
                }

                try
                {
                    Context.Attach(holder).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
                    await Context.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Cannot delete holder");
                    ModelState.AddModelError(string.Empty, "Cannot delete holder");
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
    }
}