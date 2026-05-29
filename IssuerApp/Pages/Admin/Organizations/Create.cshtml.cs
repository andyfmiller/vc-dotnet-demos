using IssuerApp.Data;
using IssuerApp.Data.Models;
using IssuerApp.Data.Models.OpenBadges;
using IssuerApp.Extensions;
using IssuerApp.Helpers;
using IssuerApp.Pages.Shared;
using IssuerApp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace IssuerApp.Pages.Admin.Organizations
{
    public class CreateModel : IssuerAppPageModel, IImageUploadPageModel
    {
        private readonly IDidWebService _didWebService;
        private readonly IEd25519SigningService _signingService;

        public CreateModel(
            ApplicationDbContext context,
            ILogger<CreateModel> logger,
            UserManager<ApplicationUser> userManager,
            IDidWebService didWebService,
            IEd25519SigningService signingService)
            : base(context, logger, userManager)
        {
            _didWebService = didWebService;
            _signingService = signingService;
        }

        [BindProperty]
        public required Organization Organization { get; set; }

        [BindProperty]
        [Display(Name = "Image", Description = "An image representing the organization.")]
        public string? ImageId { get; set; }

        [BindProperty, Display(Name = "Image File")]
        [FormFileExtensions(Extensions = "png,jpg,jpeg,gif,svg")]
        public IFormFile? ImageFile { get; set; }

        public Task OnGetAsync()
        {
            // Generate an Ed25519 key pair for signing credentials issued by this organization.
            var (publicKeyMultibase, privateKeyBase64) = _signingService.GenerateKeyPair();
            var slug = publicKeyMultibase[1..9]; // use first 8 chars of the key as a URL-safe slug
            var host = _didWebService.GetCurrentHost();
            var did = _didWebService.BuildOrganizationDid(host, slug);

            Organization = CreateOrganization(string.Empty);
            Organization.Profile!.Id = did;
            Organization.SigningPublicKeyMultibase = publicKeyMultibase;
            Organization.SigningPrivateKeyBase64 = privateKeyBase64;

            ImageId = Organization.Profile?.Image?.Id;
            return Task.CompletedTask;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Clear validation errors and initialize required fields
            OrganizationValidationHelper.ClearValidationErrors(Organization, ImageId, ModelState);

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Done verifying

            // Clean up the address

            if (Organization.Profile?.Address == null || Organization.Profile.Address.IsEmpty())
            {
                Organization.Profile!.Address = null;
            }

            // The Organization.Profile.Id is pre-populated as a did:web URI in OnGetAsync
            // and rendered as a readonly field, so it is always a valid did:web here.

            // Generate an Ed25519 key pair if one was not already pre-populated in OnGetAsync.
            if (string.IsNullOrEmpty(Organization.SigningPublicKeyMultibase) ||
                string.IsNullOrEmpty(Organization.SigningPrivateKeyBase64))
            {
                var (publicKeyMultibase, privateKeyBase64) = _signingService.GenerateKeyPair();
                Organization.SigningPublicKeyMultibase = publicKeyMultibase;
                Organization.SigningPrivateKeyBase64 = privateKeyBase64;
            }

            try
            {
                await Context.Organizations.AddAsync(Organization);
                await Context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Cannot create organization");
                ModelState.AddModelError(string.Empty, "Cannot create organization");
                return Page();
            }

            return RedirectToPage("Edit", new { key = Organization.OrganizationKey, message = "Created" });
        }

        public async Task<IActionResult> OnPostUploadImage()
        {
            void ClearValidationErrors(CreateModel model, ModelStateDictionary modelState)
            {
                // Ensure Profile.Type collection is initialized
                if (model.Organization?.Profile != null &&
                    (model.Organization.Profile.Type == null || !model.Organization.Profile.Type.Any()))
                {
                    model.Organization.Profile.Type = new List<string> { "Profile" };
                }
                modelState.Remove($"{nameof(Organization)}.{nameof(Organization.Profile)}.{nameof(Organization.Profile.Type)}");

                // Ensure Address.Type collection is initialized
                if (model.Organization?.Profile?.Address != null &&
                    (model.Organization.Profile.Address.Type == null || !model.Organization.Profile.Address.Type.Any()))
                {
                    model.Organization.Profile.Address.Type = new List<string> { "Address" };
                }
                modelState.Remove($"{nameof(Organization)}.{nameof(Organization.Profile)}.{nameof(Organization.Profile.Address)}.{nameof(Organization.Profile.Address.Type)}");

                // Handle Image validation
                if (string.IsNullOrWhiteSpace(model.ImageId))
                {
                    modelState.Remove($"{nameof(Organization)}.{nameof(Organization.Profile)}.{nameof(Organization.Profile.Image)}.{nameof(Organization.Profile.Image.Id)}");
                    modelState.Remove($"{nameof(Organization)}.{nameof(Organization.Profile)}.{nameof(Organization.Profile.Image)}.{nameof(Organization.Profile.Image.Type)}");
                    modelState.Remove($"{nameof(Organization)}.{nameof(Organization.Profile)}.{nameof(Organization.Profile.Image)}.{nameof(Organization.Profile.Image.ImageKey)}");
                    if (model.Organization?.Profile != null)
                    {
                        model.Organization.Profile.Image = null;
                    }
                }
                else
                {
                    if (model.Organization?.Profile?.Image == null && model.Organization?.Profile != null)
                    {
                        model.Organization.Profile.Image = new Image { Type = "Image", Id = model.ImageId };
                    }
                    else if (model.Organization?.Profile?.Image != null)
                    {
                        model.Organization.Profile.Image.Id = model.ImageId;
                        if (string.IsNullOrEmpty(model.Organization.Profile.Image.Type))
                        {
                            model.Organization.Profile.Image.Type = "Image";
                        }
                    }

                    modelState.Remove($"{nameof(Organization)}.{nameof(Organization.Profile)}.{nameof(Organization.Profile.Image)}.{nameof(Organization.Profile.Image.Id)}");
                    modelState.Remove($"{nameof(Organization)}.{nameof(Organization.Profile)}.{nameof(Organization.Profile.Image)}.{nameof(Organization.Profile.Image.Type)}");
                    modelState.Remove($"{nameof(Organization)}.{nameof(Organization.Profile)}.{nameof(Organization.Profile.Image)}.{nameof(Organization.Profile.Image.ImageKey)}");
                }

                // Clear AddressKey validation
                modelState.Remove($"{nameof(Organization)}.{nameof(Organization.Profile)}.{nameof(Organization.Profile.Address)}.{nameof(Organization.Profile.Address.AddressKey)}");
            }

            // Call the validation clearing logic before checking ModelState
            ClearValidationErrors(this, ModelState);

            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // KEY DIFFERENCE FROM EDIT: No database reload - everything is in memory
                // Ensure Organization and Profile exist in memory
                if (Organization == null)
                {
                    Organization = CreateOrganization(string.Empty);
                }

                var dataUri = await ImageUploadHelper.ProcessImageUpload(
                    this, ModelState, Logger, ClearValidationErrors);

                if (!ModelState.IsValid)
                {
                    ImageId = Organization.Profile?.Image?.Id;
                    return Page();
                }

                if (dataUri != null && Organization.Profile != null)
                {
                    Organization.Profile.Image = ImageUploadHelper.CreateOrUpdateImage(
                        Organization.Profile.Image, dataUri);
                    ImageId = dataUri;
                }

                var imageValue = ModelState.FirstOrDefault(x => x.Key == nameof(ImageId)).Value;
                if (imageValue != null)
                {
                    imageValue.RawValue = ImageId;
                }

                TempData["ImageUploaded"] = "true";
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error trying to upload image");
                ModelState.AddModelError(nameof(ImageId), e.Message);
            }

            return Page();
        }
    }
}
