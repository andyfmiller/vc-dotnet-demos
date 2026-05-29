#nullable enable

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IssuerApp.Data.Models.OpenBadges
{
    public class Image : Library.Models.OpenBadges.Image
    {
        [Key]
        [JsonIgnore]
        public int ImageKey { get; init; }

        [JsonIgnore]
        public int? AchievementKey { get; set; }

        [JsonIgnore]
        public virtual Achievement? Achievement { get; set; }

        [JsonIgnore]
        public int? AchievementCredentialKey { get; set; }

        [JsonIgnore]
        public virtual AchievementCredential? AchievementCredential { get; set; }

        [JsonIgnore]
        public int? OrganizationKey { get; set; }

        [JsonIgnore]
        public virtual Organization? Organization { get; set; }
    }
}
