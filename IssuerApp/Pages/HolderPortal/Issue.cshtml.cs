using IssuerApp.Data;
using IssuerApp.Data.Models;
using IssuerApp.Data.Models.OpenBadges;
using IssuerApp.Pages.Shared;
using IssuerApp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IssuerApp.Pages.HolderPortal
{
    public class IssueModel : IssuerAppPageModel
    {
        private readonly IExchangeService _exchangeService;

        public IssueModel(
            ApplicationDbContext context,
            ILogger<IssueModel> logger,
            UserManager<ApplicationUser> userManager,
            IExchangeService exchangeService)
            : base(context, logger, userManager)
        {
            _exchangeService = exchangeService;
        }

        public AchievementCredential? AchievementCredential { get; private set; }

        public Achievement? Achievement => AchievementCredential?.CredentialSubject?.Achievement as Achievement;
        public Member? Member => AchievementCredential?.CredentialSubject?.Member;
        public string? IssuerName => AchievementCredential?.Organization?.DisplayName ?? AchievementCredential?.Issuer?.Name;

        /// <summary>
        /// The Interaction URL (§3.7.1) generated for this credential exchange.
        /// </summary>
        public string? InteractionUrl { get; private set; }

        public async Task<IActionResult> OnGetAsync(int? key)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
                return RedirectToPage("/Login");

            if (AppUser == null) return RedirectToPage("/Logout");

            if (!await UserManager.IsInRoleAsync(AppUser, Constants.Roles.Holder))
                return RedirectToPage("/Index");

            AchievementCredential = await LoadCredentialAsync(key, AppUser.SelectedMemberKey);
            if (AchievementCredential is null)
                return NotFound();

            return Page();
        }

        /// <summary>
        /// Creates a VCALM exchange for the credential and returns the Interaction URL.
        /// The holder's DID will be proven cryptographically via DIDAuthentication
        /// during the exchange rather than being self-asserted here.
        /// </summary>
        public async Task<IActionResult> OnPostRequestAsync(int? key)
        {
            if (AppUser == null) return RedirectToPage("/Logout");

            AchievementCredential = await LoadCredentialAsync(key, AppUser.SelectedMemberKey);
            if (AchievementCredential is null)
                return NotFound();

            var exchange = _exchangeService.CreateExchange(key!.Value);

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
            InteractionUrl = $"{baseUrl}/interactions/{exchange.ExchangeId}?iuv=1";

            Logger.LogInformation(
                "Exchange {ExchangeId} created for AchievementCredentialKey={Key}. Interaction URL: {Url}",
                exchange.ExchangeId, key, InteractionUrl);

            return Page();
        }

        private Task<AchievementCredential?> LoadCredentialAsync(int? key, int? memberKey) =>
            Context.AchievementCredentials
                .Include(ac => ac.CredentialSubject)
                    .ThenInclude(cs => cs!.Achievement)
                .Include(ac => ac.CredentialSubject)
                    .ThenInclude(cs => cs!.Member)
                .Include(ac => ac.Organization)
                .Where(ac => ac.AchievementCredentialKey == key
                             && ac.CredentialSubject != null
                             && ac.CredentialSubject.MemberKey == memberKey)
                .FirstOrDefaultAsync();
    }
}
