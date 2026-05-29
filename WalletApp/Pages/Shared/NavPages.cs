using Microsoft.AspNetCore.Mvc.Rendering;

namespace WalletApp.Pages.Shared
{
    public static class NavPages
    {
        public static string Dashboard => "Index";

        public static string? DashboardNavClass(ViewContext viewContext) =>
            PageNavClass(viewContext, Dashboard);

        // Admin view

        public static string Holders => "Holders";
        
        public static string? HoldersNavClass(ViewContext viewContext) =>
            PageNavClass(viewContext, Holders);

        // Holder view

        public static string MyCredentials => "My Credentials";
        
        public static string? MyCredentialsNavClass(ViewContext viewContext) =>
            PageNavClass(viewContext, MyCredentials);

        private static string? PageNavClass(ViewContext viewContext, string page)
        {
            var activePage = viewContext.ViewData["ActivePage"] as string
                             ?? Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
            return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
        }
    }
}
