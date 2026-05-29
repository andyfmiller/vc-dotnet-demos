#nullable enable

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IssuerApp.Data.Models.OpenBadges
{
    public partial class AchievementCredential : Library.Models.OpenBadges.AchievementCredential
    {
        [Key]
        [JsonIgnore]
        public int AchievementCredentialKey { get; set; }

        [JsonIgnore]
        public int? OrganizationKey { get; set; }

        [JsonIgnore]
        public virtual Organization? Organization { get; set; }

        /// <summary>
        /// The zero-based index assigned to this credential within the issuer's
        /// Bitstring Status List.  <c>null</c> means no status entry has been
        /// allocated yet (credential was created before status lists were enabled).
        /// </summary>
        [JsonIgnore]
        public int? StatusListIndex { get; set; }

        public AchievementCredential()
        {
            Type = new[] { "VerifiableCredential", "OpenBadgeCredential" };
            Context = new[] { "https://www.w3.org/ns/credentials/v2", "https://purl.imsglobal.org/spec/ob/v3p0/context-3.0.3.json" };
        }

        public new AchievementSubject? CredentialSubject
        {
            get => base.CredentialSubject as AchievementSubject;
            set => base.CredentialSubject = value!;
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
    }
}
