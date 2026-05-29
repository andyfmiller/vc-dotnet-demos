using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace IssuerApp.Pages.Shared
{
    /// <summary>
    /// Interface for page models that support image upload functionality
    /// </summary>
    public interface IImageUploadPageModel
    {
        /// <summary>
        /// The URL or data URI of the image
        /// </summary>
        string? ImageId { get; set; }

        /// <summary>
        /// The uploaded image file
        /// </summary>
        IFormFile? ImageFile { get; set; }
    }
}
