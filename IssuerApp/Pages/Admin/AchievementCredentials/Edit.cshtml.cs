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
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace IssuerApp.Pages.Admin.AchievementCredentials
{
    public class EditModel : AchievementCredentialsPageModel, IImageUploadPageModel
    {
        private readonly IStatusListService _statusListService;

        public EditModel(
            ApplicationDbContext context,
            ILogger<EditModel> logger,
            UserManager<ApplicationUser> userManager,
            IStatusListService statusListService)
            : base(context, logger, userManager)
        {
            _statusListService = statusListService;
        }

        [BindProperty]
        public int? SourceKey { get; set; }

        [BindProperty]
        [Display(Name = "Image", Description = "An image representing the credential.")]
        public string? ImageId { get; set; }

        /// <summary>True when the credential has been allocated a status list index and is currently revoked.</summary>
        public bool IsRevoked { get; private set; }

        /// <summary>The status list index for this credential, or null if not yet allocated.</summary>
        public int? StatusListIndex { get; private set; }

        /// <summary>The URL of the status list credential so it can be linked from the UI.</summary>
        public string StatusListCredentialUrl => _statusListService.StatusListCredentialUrl;

        [BindProperty, Display(Name = "Image File")]
        [FormFileExtensions(Extensions = "png,jpg,jpeg,gif,svg")]
        public IFormFile? ImageFile { get; set; }

        public List<SelectListItem> Organizations { get; set; } = [];
        public Achievement? Achievement => AchievementCredential?.CredentialSubject?.Achievement as Achievement;
        public Member? Member => AchievementCredential?.CredentialSubject?.Member;

        public async Task<IActionResult> OnGetAsync(int? key, string? message)
        {
            ViewData["Message"] = message;

            var found = await Context.AchievementCredentials
                .Include(ac => ac.CredentialSubject)
                    .ThenInclude(cs => cs!.Achievement)
                .Include(ac => ac.CredentialSubject)
                    .ThenInclude(cs => cs!.Member)
                .Include(ac => ac.CredentialSubject)
                    .ThenInclude(cs => cs!.Source)
                .Include(ac => ac.Image)
                .FirstOrDefaultAsync(ac => ac.AchievementCredentialKey == key);

            if (found == null)
            {
                ModelState.AddModelError(string.Empty, "Achievement credential not found");
                return Page();
            }
            AchievementCredential = found;

            ImageId = AchievementCredential.Image?.Id;

            // Populate status list fields for the view.
            StatusListIndex = AchievementCredential.StatusListIndex;
            if (StatusListIndex.HasValue)
                IsRevoked = _statusListService.IsRevoked(StatusListIndex.Value);

            if (AchievementCredential.CredentialSubject?.Source is Profile sourceProfile)
            {
                var sourceOrg = await Context.Organizations
                    .SingleOrDefaultAsync(o => o.ProfileKey == sourceProfile.ProfileKey);
                SourceKey = sourceOrg?.OrganizationKey;
            }

            Organizations = await GetOrganizationSelectList();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Organizations = await GetOrganizationSelectList();

            ModelState.RemoveKeys("AchievementCredential.Issuer");
            ModelState.RemoveKeys("AchievementCredential.CredentialSubject.Achievement");
            ModelState.RemoveKeys("AchievementCredential.CredentialSubject.Member");
            ModelState.Remove("AchievementCredential.CredentialSubject.Type");

            if (!ModelState.IsValid) return Page();

            if (SourceKey != null)
            {
                var sourceOrg = await Context.Organizations.FindAsync(SourceKey.Value);
                if (AchievementCredential.CredentialSubject != null)
                    AchievementCredential.CredentialSubject.Source = sourceOrg?.Profile;
            }
            else if (AchievementCredential.CredentialSubject != null)
            {
                AchievementCredential.CredentialSubject.Source = null;
            }

            if (AchievementCredential.CredentialSubject != null)
            {
                // Load the immutable FKs from the database; never trust form data for these.
                var existingSubject = await Context.AchievementSubjects
                    .AsNoTracking()
                    .Where(s => s.AchievementSubjectKey == AchievementCredential.CredentialSubject.AchievementSubjectKey)
                    .Select(s => new { s.AchievementKey, s.MemberKey })
                    .FirstOrDefaultAsync();

                AchievementCredential.CredentialSubject.AchievementKey = existingSubject?.AchievementKey;
                AchievementCredential.CredentialSubject.MemberKey = existingSubject?.MemberKey;
                AchievementCredential.CredentialSubject.Achievement = null;
            }

            var existingImage = await Context.AchievementCredentials
                .Where(ac => ac.AchievementCredentialKey == AchievementCredential.AchievementCredentialKey)
                .Select(ac => (Image?)ac.Image)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(ImageId))
            {
                AchievementCredential.Image = null;
                if (existingImage != null)
                    Context.Entry(existingImage).State = EntityState.Deleted;
            }
            else if (existingImage == null)
            {
                AchievementCredential.Image = new Image { Type = "Image", Id = ImageId };
            }
            else
            {
                existingImage.Id = ImageId;
                if (string.IsNullOrEmpty(existingImage.Type))
                    existingImage.Type = "Image";
                AchievementCredential.Image = existingImage;
                Context.Entry(existingImage).State = EntityState.Modified;
            }

            try
            {
                Context.Attach(AchievementCredential).State = EntityState.Modified;
                var acEntry = Context.Entry(AchievementCredential);
                acEntry.Property(e => e.Issuer).IsModified = false;
                acEntry.Property(e => e.OrganizationKey).IsModified = false;

                if (AchievementCredential.CredentialSubject != null)
                {
                    var csEntry = Context.Entry(AchievementCredential.CredentialSubject);
                    csEntry.State = EntityState.Modified;
                    csEntry.Property(cs => cs.Id).IsModified = false;
                    csEntry.Property(cs => cs.AchievementKey).IsModified = false;
                    csEntry.Property(cs => cs.MemberKey).IsModified = false;
                }

                await Context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Cannot save achievement credential changes.");
                ModelState.AddModelError(string.Empty, "Cannot save achievement credential changes.");
                return Page();
            }

            return RedirectToPage(new { key = AchievementCredential.AchievementCredentialKey, message = "Saved" });
        }

        public async Task<IActionResult> OnPostUploadImage()
        {
            void ClearValidationErrors(EditModel model, ModelStateDictionary modelState)
            {
                modelState.RemoveKeys("AchievementCredential.Issuer");
                modelState.RemoveKeys("AchievementCredential.CredentialSubject.Achievement");
                modelState.RemoveKeys("AchievementCredential.CredentialSubject.Member");
                modelState.Remove("AchievementCredential.CredentialSubject.Type");

                if (model.AchievementCredential?.Image != null &&
                    string.IsNullOrEmpty(model.AchievementCredential.Image.Type))
                {
                    model.AchievementCredential.Image.Type = "Image";
                }
                modelState.Remove("AchievementCredential.Image.Type");

                if (string.IsNullOrWhiteSpace(model.ImageId))
                {
                    modelState.Remove("AchievementCredential.Image.Id");
                    modelState.Remove("AchievementCredential.Image.ImageKey");
                    if (model.AchievementCredential?.Image != null)
                        model.AchievementCredential.Image = null;
                }
                else
                {
                    if (model.AchievementCredential?.Image == null)
                        model.AchievementCredential!.Image = new Image { Type = "Image", Id = model.ImageId };
                    else
                    {
                        model.AchievementCredential.Image.Id = model.ImageId;
                        if (string.IsNullOrEmpty(model.AchievementCredential.Image.Type))
                            model.AchievementCredential.Image.Type = "Image";
                    }
                    modelState.Remove("AchievementCredential.Image.Id");
                    modelState.Remove("AchievementCredential.Image.ImageKey");
                }
            }

            ClearValidationErrors(this, ModelState);

            Organizations = await GetOrganizationSelectList();

            if (!ModelState.IsValid)
                return Page();

            try
            {
                var dataUri = await ImageUploadHelper.ProcessImageUpload(
                    this, ModelState, Logger, ClearValidationErrors);

                if (!ModelState.IsValid)
                {
                    ImageId = AchievementCredential.Image?.Id;
                    return Page();
                }

                if (dataUri != null)
                {
                    AchievementCredential.Image = ImageUploadHelper.CreateOrUpdateImage(
                        AchievementCredential.Image, dataUri);
                    ImageId = dataUri;
                }

                var imageValue = ModelState.FirstOrDefault(x => x.Key == nameof(ImageId)).Value;
                if (imageValue != null)
                    imageValue.RawValue = ImageId;

                TempData["ImageUploaded"] = "true";
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error trying to upload image");
                ModelState.AddModelError(nameof(ImageId), e.Message);
            }

            // Reload nav properties using AchievementSubjectKey since FKs are not posted.
            if (AchievementCredential.CredentialSubject != null)
            {
                var subject = AchievementCredential.CredentialSubject;
                var existing = await Context.AchievementSubjects
                    .Include(s => s.Achievement)
                    .Include(s => s.Member)
                    .FirstOrDefaultAsync(s => s.AchievementSubjectKey == subject.AchievementSubjectKey);

                if (existing != null)
                {
                    subject.AchievementKey = existing.AchievementKey;
                    subject.MemberKey = existing.MemberKey;
                    subject.Achievement = existing.Achievement;
                    subject.Member = existing.Member;
                }
            }

            return Page();
        }

        // -----------------------------------------------------------------
        // Credential status management handlers
        // -----------------------------------------------------------------

        /// <summary>
        /// Revokes the credential by setting its bit in the Bitstring Status List.
        /// The change is immediately visible to any verifier that fetches <c>/status-lists/1</c>.
        /// </summary>
        public async Task<IActionResult> OnPostRevokeAsync(int? key)
        {
            var credential = await Context.AchievementCredentials
                .FirstOrDefaultAsync(ac => ac.AchievementCredentialKey == key);

            if (credential is null)
                return NotFound();

            if (credential.StatusListIndex is null)
            {
                TempData["StatusMessage"] = "Error: This credential has no status list entry yet. Issue it at least once first.";
                return RedirectToPage(new { key });
            }

            _statusListService.SetStatus(credential.StatusListIndex.Value, revoked: true);

            Logger.LogInformation(
                "Credential {Key} (index={Index}) REVOKED by {User}.",
                key, credential.StatusListIndex.Value, User.Identity?.Name);

            return RedirectToPage(new { key, message = "Credential revoked. The status list has been updated." });
        }

        /// <summary>
        /// Clears the revocation bit, restoring the credential to active status.
        /// </summary>
        public async Task<IActionResult> OnPostActivateAsync(int? key)
        {
            var credential = await Context.AchievementCredentials
                .FirstOrDefaultAsync(ac => ac.AchievementCredentialKey == key);

            if (credential is null)
                return NotFound();

            if (credential.StatusListIndex is null)
            {
                TempData["StatusMessage"] = "Error: This credential has no status list entry yet.";
                return RedirectToPage(new { key });
            }

            _statusListService.SetStatus(credential.StatusListIndex.Value, revoked: false);

            Logger.LogInformation(
                "Credential {Key} (index={Index}) re-ACTIVATED by {User}.",
                key, credential.StatusListIndex.Value, User.Identity?.Name);

            return RedirectToPage(new { key, message = "Credential re-activated. The status list has been updated." });
        }
    }
}
