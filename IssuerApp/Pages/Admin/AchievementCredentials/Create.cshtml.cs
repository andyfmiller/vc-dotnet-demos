using IssuerApp.Data;
using IssuerApp.Data.Models;
using IssuerApp.Data.Models.OpenBadges;
using IssuerApp.Extensions;
using IssuerApp.Helpers;
using IssuerApp.Pages.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace IssuerApp.Pages.Admin.AchievementCredentials
{
    public class CreateModel : AchievementCredentialsPageModel, IImageUploadPageModel
    {
        public CreateModel(
            ApplicationDbContext context,
            ILogger<CreateModel> logger,
            UserManager<ApplicationUser> userManager)
            : base(context, logger, userManager) { }

        [BindProperty]
        [Required]
        [Display(Name = "Achievement", Description = "The achievement being asserted.")]
        public int? AchievementKey { get; set; }

        [BindProperty]
        [Required]
        [Display(Name = "Member", Description = "The member this credential is issued to.")]
        public int? MemberKey { get; set; }

        [BindProperty]
        public int? SourceKey { get; set; }

        [BindProperty]
        [Display(Name = "Image", Description = "An image representing the credential.")]
        public string? ImageId { get; set; }

        [BindProperty, Display(Name = "Image File")]
        [FormFileExtensions(Extensions = "png,jpg,jpeg,gif,svg")]
        public IFormFile? ImageFile { get; set; }

        public List<SelectListItem> Achievements { get; set; } = [];
        public List<SelectListItem> Members { get; set; } = [];
        public List<SelectListItem> Organizations { get; set; } = [];

        public async Task OnGetAsync(int? achievementKey, int? memberKey)
        {
            var userId = UserManager.GetUserId(User);
            var user = await Context.Users
                .Include(u => u.SelectedOrganization)
                    .ThenInclude(o => o!.Profile)
                .Include(u => u.SelectedOrganization)
                    .ThenInclude(o => o!.Achievements)
                .Include(u => u.SelectedOrganization)
                    .ThenInclude(o => o!.Members)
                .SingleAsync(u => u.Id == userId);

            AchievementKey = achievementKey;
            MemberKey = memberKey;
            Members = user.SelectedOrganization?.Members
                .OrderBy(x => x.Name)
                .Select(x => new SelectListItem(x.Name, x.MemberKey.ToString()))
                .ToList() ?? [];
            Achievements = await GetAchievementSelectList();
            Organizations = await GetOrganizationSelectList();

            AchievementCredential = new AchievementCredential
            {
                Issuer = user.SelectedOrganization?.Profile ?? new Profile { Id = string.Empty }
            };

            if (achievementKey != null)
            {
                var achievement = user.SelectedOrganization?.Achievements
                    .SingleOrDefault(x => x.AchievementKey == achievementKey);

                if (achievement != null)
                {
                    AchievementCredential.CredentialSubject = new AchievementSubject
                    {
                        Achievement = achievement,
                        Type = new List<string> { "AchievementSubject" }
                    };
                }
            }

            if (memberKey != null)
            {
                var member = user.SelectedOrganization?.Members
                    .SingleOrDefault(x => x.MemberKey == memberKey);

                if (member != null && AchievementCredential.CredentialSubject != null)
                {
                    AchievementCredential.CredentialSubject.MemberKey = member.MemberKey;
                }
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Load user and populate select lists upfront so they're available
            // for every return Page() in this handler.
            var userId = UserManager.GetUserId(User);
            var user = await Context.Users
                .Include(u => u.SelectedOrganization)
                    .ThenInclude(o => o!.Members)
                .Include(u => u.SelectedOrganization)
                    .ThenInclude(o => o!.Profile)
                .SingleAsync(u => u.Id == userId);

            Members = user.SelectedOrganization?.Members
                .OrderBy(x => x.Name)
                .Select(x => new SelectListItem(x.Name, x.MemberKey.ToString()))
                .ToList() ?? [];
            Achievements = await GetAchievementSelectList();
            Organizations = await GetOrganizationSelectList();

            // TryValidateModel runs first
            // server-side (not submitted via the form) so they don't cause false failures.
            await TryValidateModel();
            ModelState.Remove("AchievementCredential.Issuer");
            ModelState.RemoveKeys("AchievementCredential.CredentialSubject.Achievement");
            ModelState.Remove("AchievementCredential.CredentialSubject.Type");
            ModelState.RemoveKeys("AchievementCredential.CredentialSubject.Member");

            if (!ModelState.IsValid) return Page();

            AchievementCredential.Id = $"urn:uuid:{Guid.NewGuid():D}";

            var achievement = (await GetOrgAchievements())
                .Single(x => x.AchievementKey == AchievementKey);

            if (AchievementCredential.CredentialSubject == null)
            {
                AchievementCredential.CredentialSubject = new AchievementSubject
                {
                    Achievement = achievement,
                    Type = new List<string> { "AchievementSubject" }
                };
            }
            else
            {
                AchievementCredential.CredentialSubject.Achievement = achievement;
            }

            var member = user.SelectedOrganization?.Members
                .SingleOrDefault(x => x.MemberKey == MemberKey);

            if (member == null)
            {
                ModelState.AddModelError<CreateModel>(x => x.MemberKey, "The selected member cannot be found.");
                return Page();
            }

            // Set the CredentialSubject.MemberKey to the member
            if (AchievementCredential.CredentialSubject != null)
            {
                AchievementCredential.CredentialSubject.MemberKey = member.MemberKey;
            }

            AchievementCredential.Organization = user.SelectedOrganization;
            AchievementCredential.Issuer = user.SelectedOrganization?.Profile ?? new Profile { Id = string.Empty };

            if (SourceKey != null && AchievementCredential.CredentialSubject != null)
            {
                var sourceOrg = await Context.Organizations.FindAsync(SourceKey.Value); // Profile AutoIncluded
                AchievementCredential.CredentialSubject.Source = sourceOrg?.Profile;
            }

            if (!string.IsNullOrWhiteSpace(ImageId))
            {
                AchievementCredential.Image = new Image { Type = "Image", Id = ImageId };
            }

            try
            {
                Context.Add(AchievementCredential);
                await Context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Cannot create new Open Badge");
                ModelState.AddModelError(string.Empty, e.Message);
                return Page();
            }

            return RedirectToPage("Edit", new { key = AchievementCredential.AchievementCredentialKey, message = "Created" });
        }

        public async Task<IActionResult> OnPostUploadImage()
        {
            void ClearValidationErrors(CreateModel model, ModelStateDictionary modelState)
            {
                modelState.Remove("AchievementCredential.Issuer");
                modelState.RemoveKeys("AchievementCredential.CredentialSubject.Achievement");
                modelState.Remove("AchievementCredential.CredentialSubject.Type");
                modelState.Remove("AchievementCredential.ValidFrom");
                modelState.Remove("AchievementCredential.AwardedDate");

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

            var userId = UserManager.GetUserId(User);
            var user = await Context.Users
                .Include(u => u.SelectedOrganization)
                    .ThenInclude(o => o!.Members)
                .SingleAsync(u => u.Id == userId);
            Members = user.SelectedOrganization?.Members
                .OrderBy(x => x.Name)
                .Select(x => new SelectListItem(x.Name, x.MemberKey.ToString()))
                .ToList() ?? [];
            Achievements = await GetAchievementSelectList();
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

            return Page();
        }
    }
}