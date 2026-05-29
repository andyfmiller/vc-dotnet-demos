using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WalletApp.Data;
using WalletApp.Data.Models;
using WalletApp.Pages.Shared;

namespace WalletApp.Pages.Admin.Holders
{
    public class IndexModel : WalletPageModel
    {
        public IndexModel(
            ApplicationDbContext context,
            ILogger<IndexModel> logger,
            UserManager<ApplicationUser> userManager)
            : base(context, logger, userManager) { }

        public List<Data.Models.Holder> Holders { get; set; } = [];

        public async Task OnGetAsync(string? message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                ViewData["Message"] = message;
            }

            await LoadHoldersAsync();
        }

        public override async Task<IActionResult> OnPostDeleteHolder(int? key, string? pageName = null)
        {
            var result = await base.OnPostDeleteHolder(key, pageName);

            if (result is PageResult)
            {
                await LoadHoldersAsync();
            }

            return result;
        }

        private async Task LoadHoldersAsync()
        {
            Holders = await Context.Holders
                .OrderBy(x => x.Name)
                .ToListAsync();
        }
    }
}
