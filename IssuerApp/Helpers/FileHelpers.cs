using HtmlAgilityPack;
using IssuerApp.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IssuerApp.Helpers
{
    public static class FileHelpers
    {
        /// <summary>
        /// Read an uploaded file and return the contents as a stream.
        /// </summary>
        public static Stream ReadAsStream(PageModel page, IFormFile formFile)
        {
            var fieldDisplayName = string.Empty;

            // Use reflection to obtain the display name for the model
            // property associated with this IFormFile. If a display
            // name isn't found, error messages simply won't show
            // a display name.
            MemberInfo? property =
                page.GetType().GetProperty(
                    formFile.Name.Substring(
                        formFile.Name.IndexOf(".", StringComparison.Ordinal) + 1));

            if (property != null)
            {
                if (property.GetCustomAttribute(typeof(DisplayAttribute)) is DisplayAttribute
                    displayAttribute)
                {
                    fieldDisplayName = $"{displayAttribute.Name} ";
                }
            }

            // Use Path.GetFileName to obtain the file name, which will
            // strip any path information passed as part of the
            // FileName property. HtmlEncode the result in case it must
            // be returned in an error message.
            var fileName = WebUtility.HtmlEncode(Path.GetFileName(formFile.FileName));

            // Check the file length and don't bother attempting to
            // read it if the file contains no content. This check
            // doesn't catch files that only have a BOM as their
            // content, so a content length check is made later after
            // reading the file's content to catch a file that only
            // contains a BOM.
            if (formFile.Length == 0)
            {
                throw new Exception(
                    $"The {fieldDisplayName} ({fileName}) is empty.");
            }

            try
            {
                return formFile.OpenReadStream();
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"The {fieldDisplayName}file ({fileName}) upload failed. " +
                    $"Please contact the Help Desk for support. Error: {ex.Message}");
                // Log the exception
            }
        }

        /// <summary>
        /// Read an uploaded file and return the contents as a string.
        /// </summary>
        public static async Task<string> ReadAsString(PageModel page, IFormFile formFile)
        {
            var fieldDisplayName = string.Empty;
            var textContentTypes = new[]
                { "application/json", "application/ld+json", "application/clr", "text/html" };
            var textFileExtensions = new[] { ".json", ".jsonld", ".1clr" };

            // Use reflection to obtain the display name for the model
            // property associated with this IFormFile. If a display
            // name isn't found, error messages simply won't show
            // a display name.
            MemberInfo? property =
                page.GetType().GetProperty(
                    formFile.Name.Substring(
                        formFile.Name.IndexOf(".", StringComparison.Ordinal) + 1));

            if (property != null)
            {
                if (property.GetCustomAttribute(typeof(DisplayAttribute)) is DisplayAttribute
                    displayAttribute)
                {
                    fieldDisplayName = $"{displayAttribute.Name} ";
                }
            }

            // Use Path.GetFileName to obtain the file name, which will
            // strip any path information passed as part of the
            // FileName property. HtmlEncode the result in case it must
            // be returned in an error message.
            var fileName = WebUtility.HtmlEncode(Path.GetFileName(formFile.FileName));

            // Check the file length and don't bother attempting to
            // read it if the file contains no content. This check
            // doesn't catch files that only have a BOM as their
            // content, so a content length check is made later after
            // reading the file's content to catch a file that only
            // contains a BOM.
            if (formFile.Length == 0)
            {
                throw new Exception(
                    $"The {fieldDisplayName} ({fileName}) is empty.");
            }

            if (formFile.Length > 1048576)
            {
                throw new Exception(
                    $"The {fieldDisplayName} ({fileName}) exceeds 1 MB.");
            }

            try
            {
                if (textFileExtensions.Contains(Path.GetExtension(formFile.FileName)) ||
                    textContentTypes.Contains(formFile.ContentType))
                {
                    // The StreamReader is created to read files that are UTF-8 encoded.
                    // If uploads require some other encoding, provide the encoding in the
                    // using statement. To change to 32-bit encoding, change
                    // new UTF8Encoding(...) to new UTF32Encoding().
                    using var reader =
                        new StreamReader(
                            formFile.OpenReadStream(),
                            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false,
                                throwOnInvalidBytes: true),
                            detectEncodingFromByteOrderMarks: true);
                    var fileContents = await reader.ReadToEndAsync();

                    // Check the content length in case the file's only
                    // content was a BOM and the content is actually
                    // empty after removing the BOM.
                    if (fileContents.Length > 0)
                    {
                        if (formFile.ContentType.ToLower() == "text/html")
                        {
                            var htmlDocument = new HtmlDocument();
                            htmlDocument.LoadHtml(fileContents);

                            var jsonLd =
                                htmlDocument.DocumentNode.SelectSingleNode(
                                    "(//script[contains(@type, 'application/ld+json')])[1]");

                            fileContents = jsonLd?.InnerText ?? string.Empty;
                        }

                        return fileContents;
                    }

                    throw new Exception(
                        $"The {fieldDisplayName}file ({fileName}) is empty.");
                }

                return Convert.ToBase64String(formFile.OpenReadStream().ReadToEnd());
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"The {fieldDisplayName}file ({fileName}) upload failed. " +
                    $"Please contact the Help Desk for support. Error: {ex.Message}");
                // Log the exception
            }
        }
    }
}