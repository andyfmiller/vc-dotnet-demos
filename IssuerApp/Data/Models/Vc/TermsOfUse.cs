#nullable enable

using System.ComponentModel.DataAnnotations;

namespace IssuerApp.Data.Models.Vc
{
    public partial class TermsOfUse : Library.Models.Vc.TermsOfUse
    {
        [Key]
        public int TermsOfUseKey { get; init; }

        public int? OrganizationKey { get; set; }

        public virtual Organization? Organization { get; set; }
    }
}
