#nullable enable

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IssuerApp.Data.Models.OpenBadges
{
    public class Address : Library.Models.OpenBadges.Address
    {
        [Key]
        [JsonIgnore]
        public int AddressKey { get; init; }

        [JsonIgnore]
        public int? OrganizationKey { get; set; }

        [JsonIgnore]
        public virtual Organization? Organization { get; set; }
    }
}
