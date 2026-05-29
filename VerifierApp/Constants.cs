using Library.Models.OpenBadges;

namespace VerifierApp
{
    public static class Constants
    {
        public static class HttpClient
        {
            public const string Default = "default";
        }
        
        public static class Configuration
        {
            public const string BasePath = "BasePath";
        }

        public static class Roles
        {
            public const string Admin = "Admin";
            public const string Holder = "Holder";

            public static readonly string[] UserRoles = { Admin, Holder };
        }

        public static class Assets
        {
            public const string DefaultAchievementTypeImage = "achievement.svg";

            public static readonly Dictionary<string, string> AchievementTypeAssets = new()
            {
                { AchievementType.Terms.Achievement, "achievement.svg" },
                { AchievementType.Terms.Assessment, "assessment.svg" },
                { AchievementType.Terms.Assignment, "assignment.svg" },
                { AchievementType.Terms.Award, "award.svg" },
                { AchievementType.Terms.Badge, "badge.svg" },
                { AchievementType.Terms.Certificate, "certificate.svg" },
                { AchievementType.Terms.Certification, "certification.svg" },
                { AchievementType.Terms.CoCurricular, "co-curricular.svg" },
                { AchievementType.Terms.CommunityService, "community-service.svg" },
                { AchievementType.Terms.Competency, "competency.svg" },
                { AchievementType.Terms.Course, "course.svg" },
                { AchievementType.Terms.Degree, "degree.svg" },
                { AchievementType.Terms.Diploma, "diploma.svg" },
                { AchievementType.Terms.Fieldwork, "fieldwork.svg" },
                { AchievementType.Terms.License, "license.svg" },
                { AchievementType.Terms.Membership, "membership.svg" }
            };
        }
    }
}
