using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using VerifierApp.Data;
using VerifierApp.Data.Models;
using VerifierApp.Pages.Shared;

namespace VerifierApp.Pages.Admin.Requirements
{
    public class RequirementsPageModel : VerifierPageModel
    {
        protected RequirementsPageModel(
            ApplicationDbContext context,
            ILogger logger,
            UserManager<ApplicationUser> userManager)
            : base(context, logger, userManager) { }

        /// <summary>
        /// Builds the achievement type select list for Create and Edit forms.
        /// </summary>
        /// <param name="currentType">The currently selected type value, used to pre-select the matching item.</param>
        public static List<SelectListItem> GetAchievementTypeSelectList(string? currentType = null)
        {
            string[] types =
            [
                Library.Models.OpenBadges.AchievementType.Terms.Achievement,
                Library.Models.OpenBadges.AchievementType.Terms.ApprenticeshipCertificate,
                Library.Models.OpenBadges.AchievementType.Terms.Assessment,
                Library.Models.OpenBadges.AchievementType.Terms.Assignment,
                Library.Models.OpenBadges.AchievementType.Terms.AssociateDegree,
                Library.Models.OpenBadges.AchievementType.Terms.Award,
                Library.Models.OpenBadges.AchievementType.Terms.Badge,
                Library.Models.OpenBadges.AchievementType.Terms.BachelorDegree,
                Library.Models.OpenBadges.AchievementType.Terms.Certificate,
                Library.Models.OpenBadges.AchievementType.Terms.CertificateOfCompletion,
                Library.Models.OpenBadges.AchievementType.Terms.Certification,
                Library.Models.OpenBadges.AchievementType.Terms.CommunityService,
                Library.Models.OpenBadges.AchievementType.Terms.Competency,
                Library.Models.OpenBadges.AchievementType.Terms.Course,
                Library.Models.OpenBadges.AchievementType.Terms.CoCurricular,
                Library.Models.OpenBadges.AchievementType.Terms.Degree,
                Library.Models.OpenBadges.AchievementType.Terms.Diploma,
                Library.Models.OpenBadges.AchievementType.Terms.DoctoralDegree,
                Library.Models.OpenBadges.AchievementType.Terms.Fieldwork,
                Library.Models.OpenBadges.AchievementType.Terms.GeneralEducationDevelopment,
                Library.Models.OpenBadges.AchievementType.Terms.JourneymanCertificate,
                Library.Models.OpenBadges.AchievementType.Terms.LearningProgram,
                Library.Models.OpenBadges.AchievementType.Terms.License,
                Library.Models.OpenBadges.AchievementType.Terms.Membership,
                Library.Models.OpenBadges.AchievementType.Terms.MasterCertificate,
                Library.Models.OpenBadges.AchievementType.Terms.MasterDegree,
                Library.Models.OpenBadges.AchievementType.Terms.MicroCredential,
                Library.Models.OpenBadges.AchievementType.Terms.ProfessionalDoctorate,
                Library.Models.OpenBadges.AchievementType.Terms.QualityAssuranceCredential,
                Library.Models.OpenBadges.AchievementType.Terms.ResearchDoctorate,
                Library.Models.OpenBadges.AchievementType.Terms.SecondarySchoolDiploma
            ];
            return types.Select(t => new SelectListItem(t, t, t == currentType)).ToList();
        }
    }
}
