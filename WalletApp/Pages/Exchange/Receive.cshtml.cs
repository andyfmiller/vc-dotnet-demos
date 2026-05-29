using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using WalletApp.Data;
using WalletApp.Data.Models;
using WalletApp.Pages.Shared;
using WalletApp.Services;

namespace WalletApp.Pages.Exchange
{
    /// <summary>
    /// The student pastes or navigates to this page with an Interaction URL.
    /// The page calls the mock holder service to walk through the VCALM protocol
    /// and display the received credential.
    /// </summary>
    [AllowAnonymous]
    public class ReceiveModel : WalletPageModel
    {
        private readonly IHolderExchangeService _holderExchangeService;

        public ReceiveModel(
            IHolderExchangeService holderExchangeService,
            ApplicationDbContext context,
            ILogger<ReceiveModel> logger,
            UserManager<ApplicationUser> userManager)
             : base(context, logger, userManager)
        {
            _holderExchangeService = holderExchangeService;
        }

        /// <summary>
        /// The Interaction URL shared by the teacher (§3.7.1 format, includes ?iuv=1).
        /// Can be pre-filled via the query string: /Exchange/Receive?url=...
        /// </summary>
        [BindProperty]
        [Required(ErrorMessage = "Please enter the Interaction URL.")]
        [Display(Name = "Interaction URL")]
        public string? InteractionUrl { get; set; }

        public HolderExchangeResult? Result { get; private set; }

        /// <summary>
        /// Non-null after a successful receive when the incoming credential replaced a
        /// previously stored version (same <c>credentialId</c>).  Exposed so the Razor
        /// page can show a diff summary.
        /// </summary>
        public List<CredentialChange>? CredentialChanges { get; private set; }

        public void OnGet([FromQuery] string? url)
        {
            InteractionUrl = url;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            Logger.LogInformation(
                "Holder initiating exchange via Interaction URL: {Url}", InteractionUrl);

            // Resolve the selected holder (with signing key) to prove DID ownership.
            Data.Models.Holder? holder = null;
            if (User.Identity is { IsAuthenticated: true })
            {
                if (AppUser != null)
                {
                    holder = AppUser.SelectedHolder;
                }
            }

            Result = await _holderExchangeService.ReceiveCredentialAsync(InteractionUrl!, holder);

            if (Result.Success)
            {
                await PersistCredentialAsync(Result);
            }

            return Page();
        }

        private async Task PersistCredentialAsync(HolderExchangeResult result)
        {
            // Extract the first verifiableCredential from the VP
            if (string.IsNullOrEmpty(result.VerifiablePresentationJson))
                return;

            try
            {
                using var vpDoc = JsonDocument.Parse(result.VerifiablePresentationJson);
                var root = vpDoc.RootElement;

                JsonElement credentialElement;
                if (root.TryGetProperty("verifiableCredential", out var vcProp))
                {
                    if (vcProp.ValueKind == JsonValueKind.Array && vcProp.GetArrayLength() > 0)
                        credentialElement = vcProp[0];
                    else if (vcProp.ValueKind == JsonValueKind.Object)
                        credentialElement = vcProp;
                    else
                        return;
                }
                else
                {
                    return;
                }

                var credentialJson = JsonSerializer.Serialize(credentialElement, new JsonSerializerOptions { WriteIndented = true });

                string? credentialId = null;
                if (credentialElement.TryGetProperty("id", out var idProp))
                    credentialId = idProp.GetString();

                // Resolve the holder for the current user (if authenticated)
                int? holderKey = null;
                if (User.Identity is { IsAuthenticated: true })
                {
                    if (AppUser != null)
                    {
                        holderKey = AppUser.SelectedHolderKey;
                    }
                }

                // Replace or skip duplicate credential ids
                if (!string.IsNullOrEmpty(credentialId))
                {
                    var existing = await Context.HolderCredentials
                        .FirstOrDefaultAsync(c => c.CredentialId == credentialId && c.HolderKey == holderKey);

                    if (existing != null)
                    {
                        if (existing.CredentialJson == credentialJson)
                        {
                            ViewData["Message"] = "Credential is already up to date — no changes detected.";
                            return;
                        }

                        // Credential has changed: archive the old version and replace
                        existing.PreviousCredentialJson = existing.CredentialJson;
                        existing.ReplacedAt = DateTimeOffset.UtcNow;
                        existing.CredentialJson = credentialJson;

                        CredentialChanges = existing.GetChanges();

                        await Context.SaveChangesAsync();

                        Logger.LogInformation(
                            "Credential {Id} updated for holder {HolderKey} with {Count} change(s).",
                            credentialId, holderKey, CredentialChanges.Count);
                        return;
                    }
                }

                Context.HolderCredentials.Add(new HolderCredential
                {
                    HolderKey = holderKey,
                    CredentialJson = credentialJson,
                    CredentialId = credentialId
                });

                await Context.SaveChangesAsync();
            }
            catch (JsonException ex)
            {
                Logger.LogWarning(ex, "Could not parse credential JSON for persistence.");
            }
        }
    }
}
