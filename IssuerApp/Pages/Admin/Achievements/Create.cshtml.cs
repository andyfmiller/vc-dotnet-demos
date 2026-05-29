using IssuerApp.Data;
using IssuerApp.Data.Models;
using IssuerApp.Data.Models.OpenBadges;
using IssuerApp.Helpers;
using IssuerApp.Pages.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace IssuerApp.Pages.Admin.Achievements
{
    public class CreateModel : AchievementsPageModel, IImageUploadPageModel
    {
        public CreateModel(
            ApplicationDbContext context,
            ILogger<CreateModel> logger,
            UserManager<ApplicationUser> userManager)
            : base(context, logger, userManager) { }

        [BindProperty]
        [Display(Name = "Image", Description = "An image representing the achievement.")]
        public string? ImageId { get; set; }

        [BindProperty, Display(Name = "Image File")]
        [FormFileExtensions(Extensions = "png,jpg,jpeg,gif,svg")]
        public IFormFile? ImageFile { get; set; }

        public List<SelectListItem> AchievementTypes { get; private set; } = [];

        public IActionResult OnGet()
        {
            if (AppUser?.SelectedOrganization == null)
            {
                return RedirectToPage("Index", new { message = "Please select an organization." });
            }

            AchievementTypes = GetAchievementTypeSelectList();
            Achievement = CreateAchievement();
            ImageId = Achievement.Image?.Id;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (AppUser?.SelectedOrganization == null)
            {
                return RedirectToPage("Index", new { message = "Please select an organization." });
            }

            // Repopulate select lists and apply any defaulting logic before validation
            AchievementTypes = GetAchievementTypeSelectList(Achievement.AchievementType?.Type);

            // AchievementType is optional; an empty selection posts "" which must be treated as null.
            if (string.IsNullOrEmpty(Achievement.AchievementType?.Type))
                Achievement.AchievementType = null;

            if (!Achievement.Type.Any()) Achievement.Type = ["Achievement"];

            Achievement.Criteria ??= new Criteria();

            // Ensure the achievement is associated with the user's selected organization
            Achievement.Organization = AppUser!.SelectedOrganization!;

            ModelState.Remove("Achievement.Type");
            ModelState.Remove("Achievement.AchievementType");
            ModelState.Remove("Achievement.AchievementType.Type");

            if (!await TryValidateModel()) return Page();

            if (!string.IsNullOrWhiteSpace(ImageId))
            {
                Achievement.Image = new Image { Type = "Image", Id = ImageId };
            }

            try
            {
                Context.Achievements.Add(Achievement);
                await Context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Cannot save achievement");
                ModelState.AddModelError(string.Empty, "Cannot save achievement.");
                return Page();
            }

            return RedirectToPage("Edit", new { Key = Achievement.AchievementKey, message = "Created" });
        }

        public async Task<IActionResult> OnPostUploadImage()
        {
            if (AppUser?.SelectedOrganization == null)
            {
                return RedirectToPage("Index", new { message = "Please select an organization." });
            }

            void ClearValidationErrors(CreateModel model, ModelStateDictionary modelState)
            {
                // Mirror OnPostAsync: default and clear non-form required fields
                if (model.Achievement != null)
                    model.Achievement.Criteria ??= new Criteria();
                if (model.Achievement?.Type?.Any() != true && model.Achievement != null)
                    model.Achievement.Type = ["Achievement"];
                if (string.IsNullOrEmpty(model.Achievement?.AchievementType?.Type) && model.Achievement != null)
                    model.Achievement.AchievementType = null;
                modelState.Remove("Achievement.Criteria");
                modelState.Remove("Achievement.Type");
                modelState.Remove("Achievement.AchievementType");
                modelState.Remove("Achievement.AchievementType.Type");

                // Ensure the achievement is associated with the user's selected organization
                if (model.Achievement != null)
                    model.Achievement.Organization = AppUser!.SelectedOrganization!;

                // Ensure Profile.Type collection is initialized
                if (model.Achievement?.Image != null &&
                    (model.Achievement.Image.Type == null || !model.Achievement.Image.Type.Any()))
                {
                    model.Achievement.Image.Type = "Image";
                }
                modelState.Remove($"{nameof(Achievement)}.{nameof(Achievement.Image)}.{nameof(Achievement.Image.Type)}");

                // Handle Image validation
                if (string.IsNullOrWhiteSpace(model.ImageId))
                {
                    modelState.Remove($"{nameof(Achievement)}.{nameof(Achievement.Image)}.{nameof(Achievement.Image.Id)}");
                    modelState.Remove($"{nameof(Achievement)}.{nameof(Achievement.Image)}.{nameof(Achievement.Image.Type)}");
                    modelState.Remove($"{nameof(Achievement)}.{nameof(Achievement.Image)}.{nameof(Achievement.Image.ImageKey)}");
                    if (model.Achievement?.Image != null)
                    {
                        model.Achievement.Image = null;
                    }
                }
                else
                {
                    if (model.Achievement?.Image == null)
                    {
                        model.Achievement!.Image = new Image { Type = "Image", Id = model.ImageId };
                    }
                    else
                    {
                        model.Achievement.Image.Id = model.ImageId;
                        if (string.IsNullOrEmpty(model.Achievement.Image.Type))
                        {
                            model.Achievement.Image.Type = "Image";
                        }
                    }

                    modelState.Remove($"{nameof(Achievement)}.{nameof(Achievement.Image)}.{nameof(Achievement.Image.Id)}");
                    modelState.Remove($"{nameof(Achievement)}.{nameof(Achievement.Image)}.{nameof(Achievement.Image.Type)}");
                    modelState.Remove($"{nameof(Achievement)}.{nameof(Achievement.Image)}.{nameof(Achievement.Image.ImageKey)}");
                }
            }

            // Repopulate select lists and apply any defaulting logic before validation
            AchievementTypes = GetAchievementTypeSelectList(Achievement?.AchievementType?.Type);

            // Call the validation clearing logic before checking ModelState
            ClearValidationErrors(this, ModelState);

            // For image uploads, only ImageFile/ImageId errors should block the operation.
            // Clear all other field errors so that unrelated required fields (e.g. Name, Description)
            // don't prevent the image from being uploaded.
            var imageKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                { nameof(ImageId), nameof(ImageFile) };
            foreach (var key in ModelState.Keys.Where(k => !imageKeys.Contains(k)).ToList())
                ModelState.Remove(key);

            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // KEY DIFFERENCE FROM EDIT: No database reload - everything is in memory
                // Ensure Achievement exists in memory
                if (Achievement == null)
                {
                    Achievement = CreateAchievement();
                }

                var dataUri = await ImageUploadHelper.ProcessImageUpload(
                    this, ModelState, Logger, ClearValidationErrors);

                if (!ModelState.IsValid)
                {
                    ImageId = Achievement.Image?.Id;
                    return Page();
                }

                if (dataUri != null)
                {
                    Achievement.Image = ImageUploadHelper.CreateOrUpdateImage(
                        Achievement.Image, dataUri);
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