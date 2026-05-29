using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VerifierApp.Data;
using VerifierApp.Data.Models;
using VerifierApp.Pages.Shared;
using VerifierApp.Services;

namespace VerifierApp.Pages.HolderPortal
{
    /// <summary>
    /// Holder-facing page that initiates a VCALM verification exchange.
    ///
    /// GET  — displays a list of credential requirements for the holder to choose from.
    /// POST — creates the exchange record, builds the Interaction URL, and
    ///        displays it (and a QR code placeholder) for the Holder to use in WalletApp.
    /// </summary>
    [AllowAnonymous]
    public class RequireModel : VerifierPageModel
    {
        private readonly IVerificationExchangeService _exchangeService;

        public RequireModel(
            ApplicationDbContext context,
            ILogger<RequireModel> logger,
            UserManager<ApplicationUser> userManager,
            IVerificationExchangeService exchangeService)
            : base(context, logger, userManager)
        {
            _exchangeService = exchangeService;
        }

        /// <summary>The requirements available for the holder to choose from.</summary>
        public List<CredentialRequirement> RequirementOptions { get; private set; } = [];

        /// <summary>The selected requirement, populated after a successful POST.</summary>
        public CredentialRequirement? SelectedRequirement { get; private set; }

        /// <summary>The key of the requirement chosen by the holder.</summary>
        [BindProperty]
        public int? SelectedRequirementKey { get; set; }

        /// <summary>The Interaction URL generated for this verification exchange.</summary>
        public string? InteractionUrl { get; private set; }

        /// <summary>The exchange ID, kept so the page can poll for a result (future enhancement).</summary>
        public string? ExchangeId { get; private set; }

        /// <summary>The completed exchange record, populated once the wallet has responded.</summary>
        public VerificationExchangeRecord? CompletedExchange { get; private set; }

        public async Task OnGetAsync()
        {
            await LoadRequirementOptionsAsync();
        }

        /// <summary>
        /// Called by the polling script to check if an exchange has completed.
        /// Returns the page with <see cref="CompletedExchange"/> populated when done.
        /// </summary>
        public async Task OnGetResultAsync(string exchangeId)
        {
            await LoadRequirementOptionsAsync();
            ExchangeId = exchangeId;

            var exchange = _exchangeService.GetExchange(exchangeId);
            if (exchange?.State == "complete")
                CompletedExchange = exchange;
        }

        /// <summary>
        /// Creates a verification exchange for the selected requirement and returns the Interaction URL.
        /// </summary>
        public async Task<IActionResult> OnPostStartAsync()
        {
            await LoadRequirementOptionsAsync();

            if (SelectedRequirementKey is null)
            {
                ModelState.AddModelError(nameof(SelectedRequirementKey), "Please select a requirement.");
                return Page();
            }

            var requirement = await Context.Requirements
                .FirstOrDefaultAsync(r => r.CredentialRequirementKey == SelectedRequirementKey);

            if (requirement is null)
            {
                ModelState.AddModelError(nameof(SelectedRequirementKey), "The selected requirement was not found.");
                return Page();
            }

            SelectedRequirement = requirement;

            var exchange = _exchangeService.CreateExchange(requirement);

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
            InteractionUrl = $"{baseUrl}/interactions/{exchange.ExchangeId}?iuv=1";
            ExchangeId = exchange.ExchangeId;

            Logger.LogInformation(
                "Verification exchange {ExchangeId} created for requirement '{Name}'. Interaction URL: {Url}",
                exchange.ExchangeId, requirement.Name, InteractionUrl);

            return Page();
        }

        private async Task LoadRequirementOptionsAsync()
        {
            RequirementOptions = await Context.Requirements
                .OrderBy(r => r.Name)
                .ToListAsync();
        }
    }
}
