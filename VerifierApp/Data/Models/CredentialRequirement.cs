using System.ComponentModel.DataAnnotations;

namespace VerifierApp.Data.Models
{
    public class CredentialRequirement
    {
        [Key]
        public int? CredentialRequirementKey { get; set; }

        [Display(Name = "Name", Description = "A short name for this requirement. This may be displayed to the user.")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Reason", Description = "The reason for the requirement. This may be displayed to the user.")]
        public string Reason { get; set; } = string.Empty;

        [Display(Name="Achievement Type", Description = "The type of achievement required.")]
        public string AchievementType { get; set; } = string.Empty;

        [Display(Name = "Credential Type", Description = "The type of credential required.")]
        public string CredentialType { get; set; } = string.Empty;
    }
}
