#nullable enable

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IssuerApp.Data.Models.OpenBadges
{
    public partial class AchievementSubject : Library.Models.OpenBadges.AchievementSubject
    {
        [Key]
        [JsonIgnore]
        public int AchievementSubjectKey { get; init; }

        [JsonIgnore]
        public int? AchievementKey { get; set; }

        /// <summary>
        /// Override Achievement to use IssuerApp's Achievement type with AchievementKey.
        /// </summary>
        [Required]
        [Display(Name = "Achievement", Description = "The achievement being asserted.")]
        public new Achievement? Achievement
        {
            get => base.Achievement as Achievement;
            set => base.Achievement = value;
        }

        [JsonIgnore]
        public int? SourceProfileKey { get; set; }

        /// <summary>
        /// Override Source to use IssuerApp's Profile type with ProfileKey.
        /// </summary>
        public new Profile? Source
        {
            get => base.Source as Profile;
            set => base.Source = value;
        }

        [JsonIgnore]
        public int? MemberKey { get; set; }

        /// <summary>
        /// The member this credential was issued to, tracked as a FK
        /// so the member's name is available after issuance.
        /// </summary>
        [JsonIgnore]
        [Required]
        [Display(Name = "Member", Description = "The member this credential was issued to.")]
        public virtual Member? Member { get; set; }
    }
}
