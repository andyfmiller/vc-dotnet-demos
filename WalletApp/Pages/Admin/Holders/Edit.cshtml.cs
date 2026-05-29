using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WalletApp.Data;
using WalletApp.Data.Models;
using WalletApp.Pages.Shared;

namespace WalletApp.Pages.Admin.Holders
{
    public class EditModel : WalletPageModel
    {
        public EditModel(
            ApplicationDbContext context,
            ILogger<EditModel> logger,
            UserManager<ApplicationUser> userManager)
            : base(context, logger, userManager) { }

        [BindProperty]
        public required Data.Models.Holder Holder { get; set; }

        public async Task<IActionResult> OnGetAsync(int? key, string? message)
        {
            ViewData["Message"] = message;

            var found = await Context.Holders.FindAsync(key);

            if (found == null)
            {
                ModelState.AddModelError(string.Empty, "Cannot find the holder.");
                return Page();
            }

            Holder = found;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var existing = await Context.Holders.FindAsync(Holder.HolderKey);

            if (existing == null)
            {
                ModelState.AddModelError(string.Empty, "Cannot find the holder.");
                return Page();
            }

            existing.Id = Holder.Id;
            existing.Name = Holder.Name;

            try
            {
                await Context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Cannot update holder");
                ModelState.AddModelError(string.Empty, "Cannot update holder");
                return Page();
            }

            return RedirectToPage("Index", new { message = $"Holder '{Holder.Name}' updated successfully." });
        }
    }
}
