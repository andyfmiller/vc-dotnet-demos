#nullable enable

using System.ComponentModel.DataAnnotations;

namespace IssuerApp.Data.Models.OpenBadges
{
    public class IdentityObject : Library.Models.OpenBadges.IdentityObject
    {
        [Key]
        public int IdentityObjectKey { get; init; }

        public int? OrganizationKey { get; set; }

        public virtual Organization? Organization { get; set; }
    }
}
