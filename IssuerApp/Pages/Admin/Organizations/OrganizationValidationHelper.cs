using IssuerApp.Data.Models;
using IssuerApp.Data.Models.OpenBadges;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System.Linq;

namespace IssuerApp.Pages.Admin.Organizations
{
    /// <summary>
    /// Helper methods specific to Organization entity validation
    /// </summary>
    public static class OrganizationValidationHelper
    {
        /// <summary>
        /// Clear validation errors and initialize required fields for Organization entity.
        /// This is the common validation clearing logic used in both OnPostAsync and OnPostUploadImage.
        /// </summary>
        /// <param name="organization">The organization being edited</param>
        /// <param name="imageId">The current image ID</param>
        /// <param name="modelState">ModelState dictionary to clear errors from</param>
        public static void ClearValidationErrors(Data.Models.Organization organization, string? imageId, ModelStateDictionary modelState)
        {
            // Ensure Profile.Type collection is initialized (required by model but not bound from form)
            if (organization?.Profile != null && (organization.Profile.Type == null || !organization.Profile.Type.Any()))
            {
                organization.Profile.Type = new List<string> { "Profile" };
            }
            modelState.Remove($"{nameof(Data.Models.Organization)}.{nameof(Data.Models.Organization.Profile)}.{nameof(Library.Models.OpenBadges.Profile.Type)}");

            // Ensure Address.Type collection is initialized (required by model but not bound from form)
            if (organization?.Profile?.Address != null && (organization.Profile.Address.Type == null || !organization.Profile.Address.Type.Any()))
            {
                organization.Profile.Address.Type = new List<string> { "Address" };
            }
            modelState.Remove($"{nameof(Data.Models.Organization)}.{nameof(Data.Models.Organization.Profile)}.{nameof(Library.Models.OpenBadges.Profile.Address)}.{nameof(Library.Models.OpenBadges.Address.Type)}");

            // Handle Image.Id validation - if blank, set Image to null; if has value, validate and set it
            if (string.IsNullOrWhiteSpace(imageId))
            {
                // Clear all validation errors for Image properties when Image is being removed
                modelState.Remove($"{nameof(Data.Models.Organization)}.{nameof(Data.Models.Organization.Profile)}.{nameof(Library.Models.OpenBadges.Profile.Image)}.{nameof(Image.Id)}");
                modelState.Remove($"{nameof(Data.Models.Organization)}.{nameof(Data.Models.Organization.Profile)}.{nameof(Library.Models.OpenBadges.Profile.Image)}.{nameof(Image.Type)}");
                modelState.Remove($"{nameof(Data.Models.Organization)}.{nameof(Data.Models.Organization.Profile)}.{nameof(Library.Models.OpenBadges.Profile.Image)}.{nameof(Image.ImageKey)}");
                if (organization?.Profile != null)
                {
                    organization.Profile.Image = null;
                }
            }
            else
            {
                // Image.Id has a value, ensure Image object exists and set the Id
                if (organization?.Profile?.Image == null && organization?.Profile != null)
                {
                    organization.Profile.Image = new Image { Type = "Image", Id = imageId };
                }
                else if (organization?.Profile?.Image != null)
                {
                    organization.Profile.Image.Id = imageId;
                    // Ensure Image.Type is set
                    if (string.IsNullOrEmpty(organization.Profile.Image.Type))
                    {
                        organization.Profile.Image.Type = "Image";
                    }
                }

                // Clear ModelState errors for Image properties since we've initialized them
                modelState.Remove($"{nameof(Data.Models.Organization)}.{nameof(Data.Models.Organization.Profile)}.{nameof(Library.Models.OpenBadges.Profile.Image)}.{nameof(Image.Id)}");
                modelState.Remove($"{nameof(Data.Models.Organization)}.{nameof(Data.Models.Organization.Profile)}.{nameof(Library.Models.OpenBadges.Profile.Image)}.{nameof(Image.Type)}");
                modelState.Remove($"{nameof(Data.Models.Organization)}.{nameof(Data.Models.Organization.Profile)}.{nameof(Library.Models.OpenBadges.Profile.Image)}.{nameof(Image.ImageKey)}");
            }

            // Clear ModelState error for AddressKey since it's a key field and may have invalid empty string
            modelState.Remove($"{nameof(Data.Models.Organization)}.{nameof(Data.Models.Organization.Profile)}.{nameof(Library.Models.OpenBadges.Profile.Address)}.{nameof(Address.AddressKey)}");
        }
    }
}
