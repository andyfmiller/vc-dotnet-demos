#nullable enable

using System.ComponentModel.DataAnnotations;

namespace IssuerApp.Data.Models.Vc
{
    public partial class VerifiableCredential<TSubject, TIssuer> : Library.Models.Vc.VerifiableCredential<TSubject, TIssuer>
        where TSubject : Library.Models.Vc.CredentialSubject 
        where TIssuer : Library.Models.Vc.Issuer 
    {
        [Key]
        public int VerifiableCredentialKey { get; init; }

        public int? OrganizationKey { get; set; }

        public virtual Organization? Organization { get; set; }
    }
}
