using IssuerApp.Data;
using IssuerApp.Data.Models;
using IssuerApp.Pages.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IssuerApp.Pages.Admin.Members
{
    public class CreateModel : IssuerAppPageModel
    {
        public CreateModel(
            ApplicationDbContext context,
            ILogger<CreateModel> logger,
            UserManager<ApplicationUser> userManager)
            : base(context, logger, userManager) { }

        [BindProperty]
        public required Member Member { get; set; }

        public IActionResult OnGet()
        {
            if (AppUser?.SelectedOrganization == null)
            {
                return RedirectToPage("Index", new { message = "Please select an organization." });
            }

            Member = new Member
            {
                Name = string.Empty,
                Organization = AppUser.SelectedOrganization,
                OrganizationKey = AppUser.SelectedOrganizationKey!.Value
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Ensure the member is associated with the user's selected organization
            Member.Organization = AppUser!.SelectedOrganization!;

            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                Context.Members.Add(Member);
                await Context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Cannot add member.");
                ModelState.AddModelError(string.Empty, "Cannot add member");
                return Page();
            }

            return RedirectToPage("Edit", new { Key = Member.MemberKey, message = "Created" });
        }
    }
}
