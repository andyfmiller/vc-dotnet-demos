using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Web;

namespace WalletApp.Helpers
{
    /// <summary>
    /// Generates a tooltip with the description of the element
    /// </summary>
    [HtmlTargetElement("description")]
    public class DescriptionTagHelper : TagHelper
    {
        private const string ForAttributeName = "asp-for";

        private const string Template =
            @"<span data-toggle='tooltip' data-html='true' title='{0}'>
                <i class='fas fa-info-circle text-muted'></i>
              </span>";

        [HtmlAttributeName(ForAttributeName)]
        public ModelExpression? For { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            await base.ProcessAsync(context, output);

            var description = output.Content.IsModified
                ? output.Content.GetContent()
                : (await output.GetChildContentAsync()).GetContent();

            if (string.IsNullOrEmpty(description))
            {
                if (For == null)
                {
                    return;
                }

                var metadata =
                    (Microsoft.AspNetCore.Mvc.ModelBinding.Metadata.DefaultModelMetadata?)For
                        .ModelExplorer.Metadata;

                if (metadata is null) return;

                foreach (var attribute in metadata.Attributes.PropertyAttributes ?? [])
                {
                    if (attribute is DescriptionAttribute descriptionAttribute)
                    {
                        if (!string.IsNullOrEmpty(descriptionAttribute.Description))
                        {
                            description = descriptionAttribute.Description;
                        }
                    }
                    else if (attribute is DisplayAttribute displayAttribute)
                    {
                        if (!string.IsNullOrEmpty(displayAttribute.Description))
                        {
                            description = displayAttribute.Description;
                        }
                    }
                }
            }

            description = HttpUtility.HtmlEncode(description);

            output.Content.SetHtmlContent(string.Format(Template, description));
        }
    }
}
