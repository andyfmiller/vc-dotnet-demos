using IssuerApp.Data;
using IssuerApp.Data.Models;
using IssuerApp.Data.Models.OpenBadges;
using IssuerApp.Pages.Shared;
using IssuerApp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IssuerApp.Pages.Admin.AchievementCredentials
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
        /// The Interaction URL (§3.7.1) generated for this credential.
        /// Displayed to the teacher after the exchange is created so they can share it
        /// with the student.
        /// </summary>
        public string? InteractionUrl { get; private set; }

        public async Task<IActionResult> OnGetAsync(int? key)
        {
            ViewData["Title"] = "Issue via VCALM";
            AchievementCredential = await LoadCredentialAsync(key);
            if (AchievementCredential is null)
                return NotFound();

            return Page();
        }

        /// <summary>
        /// Creates a VCALM exchange for the credential and returns the Interaction URL
        /// that the teacher can share with the student.
        /// </summary>
        public async Task<IActionResult> OnPostCreateExchangeAsync(int? key)
        {
            AchievementCredential = await LoadCredentialAsync(key);
            if (AchievementCredential is null)
                return NotFound();

            var exchange = _exchangeService.CreateExchange(key!.Value);

            // §3.7.1: Interaction URL format includes the ?iuv=1 query parameter
            // which tells the wallet which version of the Interaction API is in use.
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
            InteractionUrl = $"{baseUrl}/interactions/{exchange.ExchangeId}?iuv=1";

            Logger.LogInformation(
                "Exchange {ExchangeId} created for AchievementCredentialKey={Key}. Interaction URL: {Url}",
                exchange.ExchangeId, key, InteractionUrl);

            return Page();
        }

        private Task<AchievementCredential?> LoadCredentialAsync(int? key) =>
            Context.AchievementCredentials
                .Include(ac => ac.CredentialSubject)
                    .ThenInclude(cs => cs!.Achievement)
                .Include(ac => ac.CredentialSubject)
                    .ThenInclude(cs => cs!.Member)
                .Include(ac => ac.Organization)
                    .ThenInclude(o => o!.Profile)
                .FirstOrDefaultAsync(ac => ac.AchievementCredentialKey == key);
    }
}
