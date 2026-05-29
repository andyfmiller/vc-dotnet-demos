using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace WalletApp.Helpers
{
    /// <summary>
    /// Generates a tooltip with the description of the element
    /// </summary>
    [HtmlTargetElement("formfileextensions", Attributes = ForAttributeName)]
    public class FormFileExtensionsTagHelper : TagHelper
    {
        private const string ForAttributeName = "asp-for";

        [HtmlAttributeName(ForAttributeName)]
        public ModelExpression? For { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (For is null) return;
            var metadata = (DefaultModelMetadata)For.ModelExplorer.Metadata;

            var formFileExtensionsAttribute = (FormFileExtensionsAttribute?)metadata.Attributes.PropertyAttributes!
                .SingleOrDefault(x =>
                    x.GetType().IsAssignableTo(typeof(FormFileExtensionsAttribute)));

            if (formFileExtensionsAttribute == null) return;

            var list = new List<string>();

            foreach (var extension in formFileExtensionsAttribute.Extensions.Split(','))
            {
                list.Add($"<code>{extension}</code>");
            }

            output.Content.SetHtmlContent(string.Join(", ", list));
        }
    }
}
