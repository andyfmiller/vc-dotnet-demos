using IssuerApp.Data;
using IssuerApp.Data.Models;
using IssuerApp.Pages.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IssuerApp.Pages.Admin.Members
{
    public class IndexModel : IssuerAppPageModel
    {
        public IndexModel(
            ApplicationDbContext context,
            ILogger<IndexModel> logger,
            UserManager<ApplicationUser> userManager)
            : base(context, logger, userManager) { }

        public List<MemberViewModel> Members { get; set; } = [];

        public List<SelectListItem> Organizations { get; set; } = [];

        public string SelectedOrganization {get; set; } = string.Empty;

        public async Task OnGetAsync(string? message)
        {
            ViewData["Title"] = NavPages.Members;
            ViewData["ActivePage"] = NavPages.Members;
            if (!string.IsNullOrEmpty(message))
            {
                ViewData["Message"] = message;
            }
            await LoadMembersAsync();
        }


        /// <summary>
        /// Delete a member.
        /// </summary>
        /// <param name="key">The entity key to delete.</param>
        public async Task<IActionResult> OnPostDeleteMemberAsync(int? key)
        {
            if (!key.HasValue)
            {
                ModelState.AddModelError(string.Empty, "Invalid member ID");
                await LoadMembersAsync();
                return Page();
            }

            var member = await Context.Members
                .SingleOrDefaultAsync(x => x.MemberKey == key);

            if (member == null)
            {
                ModelState.AddModelError(string.Empty, "Member not found");
                await LoadMembersAsync();
                return Page();
            }

            try
            {
                Context.Members.Remove(member); // Proper deletion
                await Context.SaveChangesAsync();
                ViewData["Message"] = "Member deleted successfully";
            }
            catch (DbUpdateException ex)
            {
                Logger.LogError(ex, "Cannot delete member {MemberKey}", key);
                ModelState.AddModelError(string.Empty, "Cannot delete the member account. It may have related records.");
            }

            await LoadMembersAsync();
            return Page();
        }

        private async Task LoadMembersAsync()
        {
            Members = await Context.Members
                .Where(r => r.OrganizationKey == AppUser!.SelectedOrganizationKey)
                .OrderBy(x => x.Name)
                .Select(r => new MemberViewModel
                {
                    Member = r,
                    CredentialCount = Context.AchievementSubjects.Count(s => s.MemberKey == r.MemberKey)
                })
                .ToListAsync();

            SelectedOrganization = AppUser!.SelectedOrganization?.DisplayName ?? "n/a";
        }
    }

    public class MemberViewModel
    {
        public required Member Member { get; set; }
        public int CredentialCount { get; set; }
    }
}
