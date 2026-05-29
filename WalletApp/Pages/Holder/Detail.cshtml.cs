using Library.Models.Vc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WalletApp.Data;
using WalletApp.Data.Models;
using WalletApp.Pages.Shared;
using WalletApp.Services.VcRender;

namespace WalletApp.Pages.Holder
{
    public class DetailModel : WalletPageModel
    {
        private readonly HtmlRenderSuiteService _htmlRenderSuite;

        public DetailModel(
            ApplicationDbContext context,
            ILogger<DetailModel> logger,
            UserManager<ApplicationUser> userManager,
            HtmlRenderSuiteService htmlRenderSuite)
            : base(context, logger, userManager)
        {
            _htmlRenderSuite = htmlRenderSuite;
        }

        public VerifiableCredential<CredentialSubject, Issuer>? VerifiableCredential { get; private set; }

        /// <summary>
        /// Non-null when the credential carries a <c>TemplateRenderMethod</c> with
        /// <c>renderSuite: "html"</c> that was successfully resolved.  The Razor page
        /// uses this to build the spec-compliant sandboxed iframe host page.
        /// </summary>
        public HtmlRenderResult? HtmlRender { get; private set; }

        /// <summary>Raw credential JSON kept for the fallback property-tree view.</summary>
        public string CredentialJson { get; private set; } = string.Empty;

        /// <summary>
        /// Non-null when this credential was replaced at least once, containing the
        /// flat list of what changed in the most recent replacement.
        /// </summary>
        public List<CredentialChange>? CredentialChanges { get; private set; }

        /// <summary>
        /// The timestamp of the most recent replacement, if any.
        /// </summary>
        public DateTimeOffset? ReplacedAt { get; private set; }

        public async Task<IActionResult> OnGetAsync(int? key)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
                return RedirectToPage("/Login");

            if (AppUser == null) return RedirectToPage("/Logout");

            if (!await UserManager.IsInRoleAsync(AppUser, Constants.Roles.Holder))
                return RedirectToPage("/Index");

            var holderCredential = await Context.HolderCredentials
                .Where(c => c.HolderKey == AppUser.SelectedHolderKey && c.HolderCredentialKey == key)
                .FirstOrDefaultAsync();

            if (holderCredential is null)
                return NotFound();

            CredentialJson = holderCredential.CredentialJson;
            VerifiableCredential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(holderCredential.CredentialJson);

            if (VerifiableCredential is null)
                return NotFound();

            if (holderCredential.ReplacedAt.HasValue)
            {
                ReplacedAt = holderCredential.ReplacedAt;
                CredentialChanges = holderCredential.GetChanges();
            }

            HtmlRender = await _htmlRenderSuite.TryResolveAsync(holderCredential.CredentialJson);

            return Page();
        }
    }
}
