using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VerifierApp.Data;
using VerifierApp.Data.Models;
using VerifierApp.Pages.Shared;

namespace VerifierApp.Pages.Admin.Requirements
{
    public class EditModel : RequirementsPageModel
    {
        public EditModel(
            ApplicationDbContext context,
            ILogger<EditModel> logger,
            UserManager<ApplicationUser> userManager)
            : base(context, logger, userManager) { }

        [BindProperty]
        public required CredentialRequirement Requirement { get; set; }

        public List<SelectListItem> AchievementTypes { get; private set; } = [];

        public async Task<IActionResult> OnGetAsync(int? key, string? message)
        {
            ViewData["Message"] = message;
            AchievementTypes = GetAchievementTypeSelectList();

            var found = await Context.Requirements.FindAsync(key);

            if (found == null)
            {
                ModelState.AddModelError(string.Empty, "Cannot find the requirement.");
                return Page();
            }

            Requirement = found;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            AchievementTypes = GetAchievementTypeSelectList();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var existing = await Context.Requirements.FindAsync(Requirement.CredentialRequirementKey);

            if (existing == null)
            {
                ModelState.AddModelError(string.Empty, "Cannot find the requirement.");
                return Page();
            }

            existing.Name = Requirement.Name;
            existing.Reason = Requirement.Reason;

            try
            {
                await Context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Cannot update requirement");
                ModelState.AddModelError(string.Empty, "Cannot update requirement");
                return Page();
            }

            return RedirectToPage("Edit", new { Key = Requirement.CredentialRequirementKey, message = $"Requirement '{Requirement.Name}' updated successfully." });
        }
    }
}
