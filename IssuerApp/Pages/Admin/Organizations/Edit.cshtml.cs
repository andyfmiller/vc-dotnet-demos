using IssuerApp.Data;
using IssuerApp.Data.Models;
using IssuerApp.Data.Models.OpenBadges;
using IssuerApp.Extensions;
using IssuerApp.Helpers;
using IssuerApp.Pages.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace IssuerApp.Pages.Admin.Organizations
{
    public class EditModel : IssuerAppPageModel, IImageUploadPageModel
    {
        public EditModel(
            ApplicationDbContext context,
            ILogger<EditModel> logger,
            UserManager<ApplicationUser> userManager)
            : base(context, logger, userManager) { }

        [BindProperty]
        public required Organization Organization { get; set; }

        [BindProperty]
        [Display(Name = "Image", Description = "An image representing the organization.")]
        public string? ImageId { get; set; }

        [BindProperty, Display(Name = "Image File")]
        [FormFileExtensions(Extensions = "png,jpg,jpeg,gif,svg")]
        public IFormFile? ImageFile { get; set; }

        public async Task<IActionResult> OnGetAsync(int? key, string? message)
        {
            ViewData["Message"] = message;
            ViewData["Title"] = "Edit";
            ViewData["ActivePage"] = NavPages.Organizations;

            var found = await Context.Organizations
                .Include(x => x.Profile)
                    .ThenInclude(x => x!.Image)
                .Include(x => x.Profile)
                    .ThenInclude(x => x!.Address)
                .SingleOrDefaultAsync(x => x.OrganizationKey == key);

            if (found == null)
            {
                ModelState.AddModelError(string.Empty, "Cannot find the organization.");
                return Page();
            }

            Organization = found;

            ImageId = Organization.Profile?.Image?.Id;

            return Page();
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

            // Load the existing organization with all related entities to properly track changes
            var existingOrg = await Context.Organizations
                .Include(x => x.Profile)
                    .ThenInclude(x => x!.Address)
                .Include(x => x.Profile)
                    .ThenInclude(x => x!.Image)
                .FirstOrDefaultAsync(x => x.OrganizationKey == Organization!.OrganizationKey);

            if (existingOrg == null)
            {
                ModelState.AddModelError(string.Empty, "Cannot find the organization.");
                return Page();
            }

            if (Organization.Profile == null)
            {
                ModelState.AddModelError(string.Empty, "Organization profile data is missing.");
                return Page();
            }

            try
            {
                // Update the existing tracked Profile with values from the form-bound organization
                if (existingOrg.Profile != null)
                {
                    existingOrg.Profile.Id = Organization.Profile.Id;
                    existingOrg.Profile.Type = Organization.Profile.Type;
                    existingOrg.Profile.Name = Organization.Profile.Name;
                    existingOrg.Profile.Url = Organization.Profile.Url;
                    existingOrg.Profile.Phone = Organization.Profile.Phone;
                    existingOrg.Profile.Description = Organization.Profile.Description;
                    existingOrg.Profile.Email = Organization.Profile.Email;
                    existingOrg.Profile.Official = Organization.Profile.Official;
                    Context.Entry(existingOrg.Profile).State = EntityState.Modified;
                }

                // Handle Address updates
                if (Organization.Profile.Address != null)
                {
                    if (Organization.Profile.Address.AddressKey == 0)
                    {
                        // New address
                        if (!Organization.Profile.Address.IsEmpty())
                        {
                            existingOrg.Profile!.Address = Organization.Profile.Address;
                            Context.Entry(Organization.Profile.Address).State = EntityState.Added;
                        }
                    }
                    else
                    {
                        // Existing address
                        if (Organization.Profile.Address.IsEmpty())
                        {
                            // Delete the address
                            if (existingOrg.Profile!.Address != null)
                            {
                                Context.Entry(existingOrg.Profile.Address).State = EntityState.Deleted;
                                existingOrg.Profile.Address = null;
                            }
                        }
                        else
                        {
                            // Update existing address
                            if (existingOrg.Profile!.Address != null)
                            {
                                existingOrg.Profile.Address.Type = Organization.Profile.Address.Type;
                                existingOrg.Profile.Address.AddressCountry = Organization.Profile.Address.AddressCountry;
                                existingOrg.Profile.Address.AddressLocality = Organization.Profile.Address.AddressLocality;
                                existingOrg.Profile.Address.AddressRegion = Organization.Profile.Address.AddressRegion;
                                existingOrg.Profile.Address.PostOfficeBoxNumber = Organization.Profile.Address.PostOfficeBoxNumber;
                                existingOrg.Profile.Address.PostalCode = Organization.Profile.Address.PostalCode;
                                existingOrg.Profile.Address.StreetAddress = Organization.Profile.Address.StreetAddress;
                                existingOrg.Profile.Address.Geo = Organization.Profile.Address.Geo;
                                Context.Entry(existingOrg.Profile.Address).State = EntityState.Modified;
                            }
                        }
                    }
                }

                // Handle Image updates
                if (Organization.Profile.Image == null)
                {
                    // Remove existing image if present
                    if (existingOrg.Profile!.Image != null)
                    {
                        Context.Entry(existingOrg.Profile.Image).State = EntityState.Deleted;
                        existingOrg.Profile.Image = null;
                    }
                }
                else if (Organization.Profile.Image.ImageKey == 0)
                {
                    // New image
                    existingOrg.Profile!.Image = Organization.Profile.Image;
                    Context.Entry(Organization.Profile.Image).State = EntityState.Added;
                }
                else
                {
                    // Update existing image
                    if (existingOrg.Profile!.Image != null)
                    {
                        existingOrg.Profile.Image.Id = Organization.Profile.Image.Id;
                        existingOrg.Profile.Image.Type = Organization.Profile.Image.Type;
                        Context.Entry(existingOrg.Profile.Image).State = EntityState.Modified;
                    }
                }

                await Context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Cannot save organization");
                ModelState.AddModelError(string.Empty, "Cannot save organization");
                return Page();
            }

            ViewData["Message"] = "Saved";
            ImageId = Organization.Profile?.Image?.Id;
            return Page();
        }

        public async Task<IActionResult> OnPostUploadImage()
        {
            void ClearValidationErrors(EditModel model, ModelStateDictionary modelState)
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
                var notValid = await Context.Organizations
                    .Include(x => x.Profile).ThenInclude(x => x!.Image)
                    .Include(x => x.Profile).ThenInclude(x => x!.Address)
                    .FirstOrDefaultAsync(x => x.OrganizationKey == Organization!.OrganizationKey);

                if (notValid != null)
                {
                    Organization = notValid;
                    ImageId = Organization.Profile?.Image?.Id;
                }

                return Page();
            }

            try
            {
                var existing = await Context.Organizations
                    .Include(x => x.Profile).ThenInclude(x => x!.Image)
                    .Include(x => x.Profile).ThenInclude(x => x!.Address)
                    .FirstOrDefaultAsync(x => x.OrganizationKey == Organization!.OrganizationKey);

                if (existing == null)
                {
                    ModelState.AddModelError(string.Empty, "Cannot find the organization.");
                    return Page();
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

