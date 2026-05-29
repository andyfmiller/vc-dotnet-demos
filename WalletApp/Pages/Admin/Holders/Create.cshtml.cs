using Library.Crypto;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WalletApp.Data;
using WalletApp.Data.Models;
using WalletApp.Pages.Shared;
using WalletApp.Services;

namespace WalletApp.Pages.Admin.Holders
{
    public class CreateModel : WalletPageModel
    {
        private readonly IDidWebHolderService _didWebService;
        private readonly IEd25519SigningService _signingService;

        public CreateModel(
            ApplicationDbContext context,
            ILogger<CreateModel> logger,
            UserManager<ApplicationUser> userManager,
            IDidWebHolderService didWebService,
            IEd25519SigningService signingService)
            : base(context, logger, userManager)
        {
            _didWebService = didWebService;
            _signingService = signingService;
        }

        [BindProperty]
        public required Data.Models.Holder Holder { get; set; }

        public Task OnGetAsync()
        {
            var (publicKeyMultibase, privateKeyBase64) = _signingService.GenerateKeyPair();
            var slug = publicKeyMultibase[1..9]; // use first 8 chars of the key as a URL-safe slug
            var host = _didWebService.GetCurrentHost();
            var did = _didWebService.BuildHolderDid(host, slug);

            Holder = new Data.Models.Holder
            {
                Id = did,
                SigningPublicKeyMultibase = publicKeyMultibase,
                SigningPrivateKeyBase64 = privateKeyBase64
            };
            return Task.CompletedTask;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                await Context.Holders.AddAsync(Holder);
                await Context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Cannot create holder");
                ModelState.AddModelError(string.Empty, "Cannot create holder");
                return Page();
            }

            return RedirectToPage("Index", new { message = $"Holder '{Holder.Name}' created successfully." });
        }
    }
}
