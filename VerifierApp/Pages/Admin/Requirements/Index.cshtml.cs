using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using VerifierApp.Data;
using VerifierApp.Data.Models;
using VerifierApp.Pages.Shared;

namespace VerifierApp.Pages.Admin.Requirements
{
    public class IndexModel : VerifierPageModel
    {
        public IndexModel(
            ApplicationDbContext context,
            ILogger<IndexModel> logger,
            UserManager<ApplicationUser> userManager)
            : base(context, logger, userManager) { }

        public List<CredentialRequirement> Requirements { get; set; } = [];

        public async Task OnGetAsync(string? message)
        {
            ViewData["Title"] = NavPages.AdminRequirements;
            ViewData["ActivePage"] = NavPages.AdminRequirements;
            if (!string.IsNullOrEmpty(message))
            {
                ViewData["Message"] = message;
            }

            await LoadRequirementsAsync();
        }

        public override async Task<IActionResult> OnPostDeleteRequirement(int? key, string? pageName = null)
        {
            var result = await base.OnPostDeleteRequirement(key, pageName);

            if (result is PageResult)
            {
                await LoadRequirementsAsync();
            }

            return result;
        }

        private async Task LoadRequirementsAsync()
        {
            Requirements = await Context.Requirements
                .OrderBy(x => x.Name)
                .ToListAsync();
        }
    }
}
