using IssuerApp.Data;
using IssuerApp.Data.Models;
using IssuerApp.Pages.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IssuerApp.Pages.Admin.Members
{
    public class EditModel : IssuerAppPageModel
    {
        public EditModel(
            ApplicationDbContext context,
            ILogger<EditModel> logger,
            UserManager<ApplicationUser> userManager)
            : base(context, logger, userManager) { }

        [BindProperty]
        public Member? Member { get; set; }

        public async Task OnGetAsync(int? key, string? message)
        {
            ViewData["Message"] = message;

            Member = await Context.Members
                .FirstOrDefaultAsync(r => r.MemberKey == key);

            if (Member == null)
            {
                ModelState.AddModelError(string.Empty, "Cannot find the member account.");
            }

            ViewData["Title"] = Member?.Name ?? "Edit Member";
            ViewData["ActivePage"] = NavPages.Members;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                if (Member == null)
                {
                    ModelState.AddModelError(string.Empty, "Cannot find the member account.");
                    return Page();
                }

                Context.Members.Update(Member);
                await Context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Cannot save member changes.");
                ModelState.AddModelError(string.Empty, "Cannot save member changes.");
                return Page();
            }

            return RedirectToPage("Edit", new { Key = Member.MemberKey, message = "Saved" });
        }
    }
}
