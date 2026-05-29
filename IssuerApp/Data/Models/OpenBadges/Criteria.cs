#nullable enable

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IssuerApp.Data.Models.OpenBadges
{
    public class Criteria : Library.Models.OpenBadges.Criteria
    {
        [Key]
        [JsonIgnore]
        public int CriteriaKey { get; init; }

        [JsonIgnore]
        public int? AchievementKey { get; set; }

        [JsonIgnore]
        public virtual Achievement? Achievement { get; set; }

        [JsonIgnore]
        public int? OrganizationKey { get; set; }

        [JsonIgnore]
        public virtual Organization? Organization { get; set; }
    }
}
