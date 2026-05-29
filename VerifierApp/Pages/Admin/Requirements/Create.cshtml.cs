using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VerifierApp.Data;
using VerifierApp.Data.Models;

namespace VerifierApp.Pages.Admin.Requirements
{
    public class CreateModel : RequirementsPageModel
    {
        public CreateModel(
            ApplicationDbContext context,
            ILogger<CreateModel> logger,
            UserManager<ApplicationUser> userManager)
            : base(context, logger, userManager) { }

        [BindProperty]
        public required CredentialRequirement Requirement { get; set; }

        public List<SelectListItem> AchievementTypes { get; private set; } = [];

        public Task OnGetAsync()
        {
            AchievementTypes = GetAchievementTypeSelectList();
            Requirement = new CredentialRequirement();
            return Task.CompletedTask;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            AchievementTypes = GetAchievementTypeSelectList();
            Requirement.CredentialType = "OpenBadgeCredential"; // For now we only support one type, but this is where you'd set it based on user input if there were multiple options.

            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                await Context.Requirements.AddAsync(Requirement);
                await Context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Cannot create requirement");
                ModelState.AddModelError(string.Empty, "Cannot create requirement");
                return Page();
            }

            return RedirectToPage("Edit", new { Key = Requirement.CredentialRequirementKey, message = $"Requirement '{Requirement.Name}' created successfully." });
        }
    }
}
