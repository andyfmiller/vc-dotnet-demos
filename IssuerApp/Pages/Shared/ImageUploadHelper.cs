using IssuerApp.Data.Models.OpenBadges;
using IssuerApp.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IssuerApp.Pages.Shared
{
    /// <summary>
    /// Helper methods for image upload functionality
    /// </summary>
    public static class ImageUploadHelper
    {
        /// <summary>
        /// Process an uploaded image file and convert it to a data URI
        /// </summary>
        /// <param name="pageModel">The page model implementing IImageUploadPageModel</param>
        /// <param name="modelState">The ModelState from the page</param>
        /// <param name="logger">Logger for error reporting</param>
        /// <param name="clearValidationErrors">Optional action to clear entity-specific validation errors before checking ModelState.IsValid. 
        /// This is where you initialize required collections and clear validation errors for fields not submitted by the modal.</param>
        /// <param name="imagePropertyName">The name of the ImageId property in ModelState (default: "ImageId")</param>
        /// <returns>The data URI of the uploaded image, or null if upload failed</returns>
        public static async Task<string?> ProcessImageUpload<TPageModel>(
            TPageModel pageModel,
            ModelStateDictionary modelState,
            ILogger logger,
            Action<TPageModel, ModelStateDictionary>? clearValidationErrors = null,
            string imagePropertyName = "ImageId")
            where TPageModel : PageModel, IImageUploadPageModel
        {
            // Allow entity-specific validation clearing (e.g., initialize required collections, clear nested property errors)
            clearValidationErrors?.Invoke(pageModel, modelState);

            if (pageModel.ImageFile == null)
            {
                modelState.AddModelError(nameof(IImageUploadPageModel.ImageFile), 
                    "Please select an image file to upload");
                return null;
            }

            if (!modelState.IsValid)
            {
                return null;
            }

            try
            {
                var base64 = await FileHelpers.ReadAsString(pageModel, pageModel.ImageFile);

                if (!modelState.IsValid)
                {
                    return null;
                }

                var dataUri = $"data:{pageModel.ImageFile.ContentType};base64,{base64}";

                // Update the ImageId in ModelState so model binding picks it up
                var imageValue = modelState.FirstOrDefault(x => x.Key == imagePropertyName).Value;
                if (imageValue != null)
                {
                    imageValue.RawValue = dataUri;
                }

                return dataUri;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error trying to upload image");
                modelState.AddModelError(imagePropertyName, e.Message);
                return null;
            }
        }

        /// <summary>
        /// Create or update an Image object with the provided data URI
        /// </summary>
        public static Image CreateOrUpdateImage(Image? existingImage, string dataUri)
        {
            if (existingImage == null)
            {
                return new Image { Type = "Image", Id = dataUri };
            }

            existingImage.Id = dataUri;
            if (string.IsNullOrEmpty(existingImage.Type))
            {
                existingImage.Type = "Image";
            }

            return existingImage;
        }
    }
}
