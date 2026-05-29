using Microsoft.AspNetCore.Mvc.Rendering;

namespace IssuerApp.Pages.Shared
{
    public static class NavPages
    {
        public static string Dashboard => "Dashboard";

        public static string? DashboardNavClass(ViewContext viewContext) =>
            PageNavClass(viewContext, Dashboard);

        // Admin view

        public static string Organizations => "Organizations";
        public static string? OrganizationsNavClass(ViewContext viewContext) =>
            PageNavClass(viewContext, Organizations);

        public static string Members => "Members";

        public static string? MembersNavClass(ViewContext viewContext) =>
            PageNavClass(viewContext, Members);

        // Teacher View

        public static string Achievements => "Achievements";

        public static string? AchievementsNavClass(ViewContext viewContext) =>
            PageNavClass(viewContext, Achievements);

        public static string AchievementCredentials => "Credentials";

        public static string? AchievementCredentialsNavClass(ViewContext viewContext) =>
            PageNavClass(viewContext, AchievementCredentials);

        private static string? PageNavClass(ViewContext viewContext, string page)
        {
            var activePage = viewContext.ViewData["ActivePage"] as string
                             ?? Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
            return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
        }
    }
}
