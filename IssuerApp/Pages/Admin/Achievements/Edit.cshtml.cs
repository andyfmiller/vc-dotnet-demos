using IssuerApp.Data;
using IssuerApp.Data.Models;
using IssuerApp.Data.Models.OpenBadges;
using IssuerApp.Helpers;
using IssuerApp.Pages.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace IssuerApp.Pages.Admin.Achievements
{
    public class EditModel : AchievementsPageModel, IImageUploadPageModel
    {
        public EditModel(
            ApplicationDbContext context,
            ILogger<EditModel> logger,
            UserManager<ApplicationUser> userManager)
            : base(context, logger, userManager) { }

        [BindProperty]
        [Display(Name = "Image", Description = "An image representing the organization.")]
        public string? ImageId { get; set; }

        [BindProperty, Display(Name = "Image File")]
        [FormFileExtensions(Extensions = "png,jpg,jpeg,gif,svg")]
        public IFormFile? ImageFile { get; set; }

        public List<SelectListItem> AchievementTypes { get; private set; } = [];

        public async Task<IActionResult> OnGetAsync(int? key, string? message)
        {
            ViewData["Message"] = message;

            if (key == null)
            {
                return RedirectToPage("Index", new { message = "Achievement not found." });
            }

            var found = await Context.Achievements
                .Include(a => a.Criteria)
                .Include(a => a.Image)
                .FirstOrDefaultAsync(a => a.AchievementKey == key);

            if (found == null)
            {
                return RedirectToPage("Index", new { message = "Achievement not found." });
            }

            Achievement = found;
            ImageId = Achievement.Image?.Id;

            AchievementTypes = GetAchievementTypeSelectList(Achievement?.AchievementType?.Type);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Repopulate select lists and apply any defaulting logic before validation
            AchievementTypes = GetAchievementTypeSelectList(Achievement?.AchievementType?.Type);

            if (Achievement != null && !Achievement.Type.Any()) Achievement.Type = ["Achievement"];
            if (Achievement == null) return Page();

            // AchievementType is optional; an empty selection posts "" which must be treated as null.
            if (string.IsNullOrEmpty(Achievement.AchievementType?.Type))
                Achievement.AchievementType = null;

            ModelState.Remove("Achievement.Type");
            ModelState.Remove("Achievement.AchievementType");
            ModelState.Remove("Achievement.AchievementType.Type");

            if (!await TryValidateModel()) return Page();

            try
            {
                if (Achievement.Criteria != null)
                {
                    Context.Entry(Achievement.Criteria).State =
                        Achievement.Criteria.CriteriaKey == 0
                            ? EntityState.Added
                            : EntityState.Modified;
                }

                Achievement.OrganizationKey = await Context.Achievements
                    .Where(a => a.AchievementKey == Achievement.AchievementKey)
                    .Select(a => a.OrganizationKey)
                    .FirstOrDefaultAsync();

                var existingImage = await Context.Achievements
                    .Where(a => a.AchievementKey == Achievement.AchievementKey)
                    .Select(a => a.Image)
                    .FirstOrDefaultAsync();

                if (string.IsNullOrWhiteSpace(ImageId))
                {
                    Achievement.Image = null;
                    if (existingImage != null)
                        Context.Entry(existingImage).State = EntityState.Deleted;
                }
                else if (existingImage == null)
                {
                    Achievement.Image = new Image { Type = "Image", Id = ImageId };
                }
                else
                {
                    existingImage.Id = ImageId;
                    if (string.IsNullOrEmpty(existingImage.Type))
                        existingImage.Type = "Image";
                    Achievement.Image = existingImage;
                    Context.Entry(existingImage).State = EntityState.Modified;
                }

                Context.Attach(Achievement).State = EntityState.Modified;

                await Context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error trying to upload image");
                ModelState.AddModelError(nameof(ImageId), e.Message);
            }

            ViewData["Message"] = "Saved";

            // TODO Fix bug that causes Organization.ParentOrganization.Profile to be null in Context cache
            // return Page();
            return RedirectToPage(new { key = Achievement.AchievementKey, message = "Saved" });
        }

        public async Task<IActionResult> OnPostUploadImage()
        {
            void ClearValidationErrors(EditModel model, ModelStateDictionary modelState)
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
                    else if (model.Achievement?.Image != null)
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

            // Repopulate AchievementTypes for redisplay in case of validation errors
            AchievementTypes = GetAchievementTypeSelectList(Achievement?.AchievementType?.Type);

            // Call the validation clearing logic before checking ModelState
            ClearValidationErrors(this, ModelState);

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

        public async Task<IActionResult> OnPostDelete()
        {
            if (Achievement == null) return RedirectToPage("Index");
            Context.Attach(Achievement).State = EntityState.Deleted;

            try
            {
                await Context.SaveChangesAsync();
                return RedirectToPage("Index");
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Cannot delete achievement");
                ModelState.AddModelError(string.Empty, "Cannot delete achievement");
                return Page();
            }
        }

        private async Task<bool> AchievementExists(int? key)
        {
            return AppUser?.SelectedOrganization?.Achievements.Any(e => e.AchievementKey == key) ?? false;
        }
    }
}