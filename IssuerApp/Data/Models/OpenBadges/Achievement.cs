#nullable enable

using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace IssuerApp.Data.Models.OpenBadges
{
    public class Achievement : Library.Models.OpenBadges.Achievement
    {
        [Key]
        [JsonIgnore]
        public int AchievementKey { get; init; }

        [JsonIgnore]
        public int OrganizationKey { get; set; }

        [JsonIgnore]
        [Display(Description = "The organization authored this achievement.")]
        [Required]
        [ValidateNever]
        public virtual Organization Organization { get; set; } = null!;

        /// <summary>
        /// Override Criteria to use IssuerApp's Criteria type with CriteriaKey.
        /// [Required] must be repeated here because 'new' hides the base property and its attributes.
        /// </summary>
        [Required]
        public new Criteria? Criteria
        {
            get => base.Criteria as Criteria;
            set => base.Criteria = value;
        }

        /// <summary>
        /// Override Image to use IssuerApp's Image type with ImageKey.
        /// Hides the base property completely to ensure IssuerApp types are used.
        /// </summary>
        public new Image? Image
        {
            get => base.Image as Image;
            set => base.Image = value;
        }

        [NotMapped]
        [JsonIgnore]
        public string DisplayName => string.IsNullOrEmpty(HumanCode) ? Name ?? string.Empty : $"{HumanCode}: {Name}";
    }
}
