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
    /// Three-stage workflow:
    ///   Stage 1 – Collect the Interaction URL; button "Find Matching Credentials".
    ///   Stage 2 – Contacts the verifier (Steps 1-3), shows matching credentials for selection.
    ///   Stage 3 – Submits the VP (Steps 4-5) and shows the result.
    /// </summary>
    [AllowAnonymous]
    public class PresentModel : WalletPageModel
    {
        private readonly IHolderVerificationService _holderVerificationService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PresentModel(
            IHolderVerificationService holderVerificationService,
            ApplicationDbContext context,
            ILogger<PresentModel> logger,
            UserManager<ApplicationUser> userManager)
            : base(context, logger, userManager)
        {
            _holderVerificationService = holderVerificationService;
            _context = context;
            _userManager = userManager;
        }

        // ── Stage 1 input ────────────────────────────────────────────────────────

        [BindProperty]
        [Display(Name = "Interaction URL")]
        public string? InteractionUrl { get; set; }

        // ── Stage 2 state (round-tripped as hidden fields) ────────────────────────

        [BindProperty]
        public string? ExchangeUrl { get; set; }

        [BindProperty]
        public new string? Challenge { get; set; }

        [BindProperty]
        public string? Domain { get; set; }

        /// <summary>Required credential types from the QBE (JSON array, stored as hidden field).</summary>
        [BindProperty]
        public string? RequiredCredentialTypesJson { get; set; }

        /// <summary>Required achievement types from the QBE (JSON array, stored as hidden field).</summary>
        [BindProperty]
        public string? RequiredAchievementTypesJson { get; set; }

        // ── Stage 2 output ────────────────────────────────────────────────────────

        /// <summary>Credentials in the wallet that match the QBE requirements.</summary>
        public List<HolderCredential> MatchingCredentials { get; private set; } = [];

        // ── Stage 3 input ─────────────────────────────────────────────────────────

        [BindProperty]
        public List<int> SelectedCredentialKeys { get; set; } = [];

        // ── Stage 3 output ────────────────────────────────────────────────────────

        public HolderVerificationResult? Result { get; private set; }

        // ── Page state helpers ────────────────────────────────────────────────────

        /// <summary>True once the interaction has been started and the QBE received.</summary>
        public bool InteractionStarted =>
            !string.IsNullOrWhiteSpace(ExchangeUrl) && !string.IsNullOrWhiteSpace(Challenge);

        public async Task OnGetAsync([FromQuery] string? url)
        {
            InteractionUrl = url;
        }

        /// <summary>
        /// "Find Matching Credentials" – runs Steps 1-3, filters the wallet, shows Stage 2.
        /// </summary>
        public async Task<IActionResult> OnPostFindAsync()
        {
            if (string.IsNullOrWhiteSpace(InteractionUrl))
            {
                ModelState.AddModelError(nameof(InteractionUrl), "Please enter the Interaction URL.");
                return Page();
            }

            var request = await _holderVerificationService.GetPresentationRequestAsync(InteractionUrl);
            if (!request.Success)
            {
                ModelState.AddModelError(string.Empty,
                    $"Could not contact the verifier: {request.ErrorMessage}");
                return Page();
            }

            // Persist QBE state for the next POST.
            ExchangeUrl = request.ExchangeUrl;
            Challenge = request.Challenge;
            Domain = request.Domain;
            RequiredCredentialTypesJson = JsonSerializer.Serialize(request.RequiredCredentialTypes);
            RequiredAchievementTypesJson = JsonSerializer.Serialize(request.RequiredAchievementTypes);

            await LoadMatchingCredentialsAsync(request.RequiredCredentialTypes, request.RequiredAchievementTypes);

            return Page();
        }

        /// <summary>
        /// "Present Selected Credentials" – runs Steps 4-5, shows Stage 3.
        /// </summary>
        public async Task<IActionResult> OnPostPresentAsync()
        {
            if (string.IsNullOrWhiteSpace(InteractionUrl))
            {
                ModelState.AddModelError(nameof(InteractionUrl), "Interaction URL is missing.");
                return Page();
            }

            if (!InteractionStarted)
            {
                ModelState.AddModelError(string.Empty, "Presentation request state is missing. Please start over.");
                return Page();
            }

            if (SelectedCredentialKeys.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "Please select at least one credential to present.");
                // Reload matching credentials so Stage 2 re-renders correctly.
                var credTypes = JsonSerializer.Deserialize<string[]>(RequiredCredentialTypesJson ?? "[]") ?? [];
                var achTypes = JsonSerializer.Deserialize<string[]>(RequiredAchievementTypesJson ?? "[]") ?? [];
                await LoadMatchingCredentialsAsync(credTypes, achTypes);
                return Page();
            }

            // Resolve the holder.
            Data.Models.Holder? holder = null;
            if (User.Identity is { IsAuthenticated: true })
            {
                if (AppUser != null)
                {
                    holder = AppUser.SelectedHolder;
                }
            }

            if (holder is null || string.IsNullOrWhiteSpace(holder.SigningPrivateKeyBase64))
            {
                ModelState.AddModelError(string.Empty,
                    "No holder with a signing key is selected. " +
                    "Please select a holder that has a generated key pair.");
                return Page();
            }

            // Load the selected credentials.
            var credentials = await _context.HolderCredentials
                .Where(c => SelectedCredentialKeys.Contains(c.HolderCredentialKey))
                .ToListAsync();

            if (credentials.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "Selected credentials not found.");
                return Page();
            }

            Logger.LogInformation(
                "Holder {HolderDid} presenting {Count} credential(s) via {Url}",
                holder.Id, credentials.Count, InteractionUrl);

            var interactionRequest = new InteractionRequestResult
            {
                Success = true,
                ExchangeUrl = ExchangeUrl,
                Challenge = Challenge,
                Domain = Domain
            };

            Result = await _holderVerificationService.PresentCredentialsAsync(
                interactionRequest, holder, credentials);

            return Page();
        }

        private async Task LoadMatchingCredentialsAsync(string[] requiredCredentialTypes, string[] requiredAchievementTypes)
        {
            int? holderKey = null;
            if (User.Identity is { IsAuthenticated: true })
            {
                if (AppUser != null)
                    holderKey = AppUser.SelectedHolderKey;
            }

            if (!holderKey.HasValue)
            {
                MatchingCredentials = [];
                return;
            }

            var all = await _context.HolderCredentials
                .Where(c => c.HolderKey == holderKey)
                .ToListAsync();

            // Filter by credential type and achievement type extracted from the QBE.
            MatchingCredentials = all
                .Where(c => MatchesRequirements(c, requiredCredentialTypes, requiredAchievementTypes))
                .OrderByDescending(c => c.ReceivedAt)
                .ToList();
        }

        private static bool MatchesRequirements(
            HolderCredential credential,
            string[] requiredCredentialTypes,
            string[] requiredAchievementTypes)
        {
            if (string.IsNullOrEmpty(credential.CredentialJson))
                return false;

            try
            {
                using var doc = JsonDocument.Parse(credential.CredentialJson);
                var root = doc.RootElement;

                // Match credential types (if the QBE specifies any).
                if (requiredCredentialTypes.Length > 0)
                {
                    var credTypes = new List<string>();
                    if (root.TryGetProperty("type", out var typeEl))
                    {
                        if (typeEl.ValueKind == JsonValueKind.Array)
                            credTypes.AddRange(typeEl.EnumerateArray().Select(t => t.GetString() ?? string.Empty));
                        else if (typeEl.ValueKind == JsonValueKind.String)
                            credTypes.Add(typeEl.GetString() ?? string.Empty);
                    }

                    if (!requiredCredentialTypes.Any(rt => credTypes.Contains(rt)))
                        return false;
                }

                // Match achievement types (if the QBE specifies any).
                if (requiredAchievementTypes.Length > 0)
                {
                    var achTypes = new List<string>();
                    if (root.TryGetProperty("credentialSubject", out var subj) &&
                        subj.TryGetProperty("achievement", out var ach) &&
                        ach.TryGetProperty("achievementType", out var achTypeEl))
                    {
                        if (achTypeEl.ValueKind == JsonValueKind.Array)
                            achTypes.AddRange(achTypeEl.EnumerateArray().Select(t => t.GetString() ?? string.Empty));
                        else if (achTypeEl.ValueKind == JsonValueKind.String)
                            achTypes.Add(achTypeEl.GetString() ?? string.Empty);
                    }

                    if (!requiredAchievementTypes.Any(at => achTypes.Contains(at)))
                        return false;
                }

                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}
