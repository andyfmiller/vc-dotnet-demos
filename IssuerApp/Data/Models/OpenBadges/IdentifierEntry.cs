#nullable enable

using System.ComponentModel.DataAnnotations;

namespace IssuerApp.Data.Models.OpenBadges
{
    public class IdentifierEntry : Library.Models.OpenBadges.IdentifierEntry
    {
        [Key]
        public int IdentifierEntryKey { get; init; }

        public int? OrganizationKey { get; set; }

        public virtual Organization? Organization { get; set; }
    }
}
