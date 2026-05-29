using Microsoft.AspNetCore.Mvc.Rendering;

namespace VerifierApp.Pages.Shared
{
    public static class NavPages
    {
        public static string AdminRequirements => "Requirements";
        public static string? AdminRequirementsNavClass(ViewContext viewContext) =>
            PageNavClass(viewContext, AdminRequirements);

        public static string HolderRequirements => "Requirements";
        public static string? HolderRequirementsNavClass(ViewContext viewContext) =>
            PageNavClass(viewContext, HolderRequirements);

        private static string? PageNavClass(ViewContext viewContext, string page)
        {
            var activePage = viewContext.ViewData["ActivePage"] as string
                             ?? Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
            return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
        }
    }
}
