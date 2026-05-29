using Library.Models.Converters;
using Library.Models.OpenBadges.Converters;
using Library.Models.Vc;
using Library.Models.Vc.Converters;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Library.Models.OpenBadges
{
    /// <summary>
    /// AchievementCredentials are representations of an awarded achievement, used to share information about a achievement belonging to one earner. Maps to a Verifiable Credential as defined in the [[VC-DATA-MODEL-2.0]]. As described in [[[#data-integrity]]], at least one proof mechanism, and the details necessary to evaluate that proof, MUST be expressed for a credential to be a verifiable credential. In the case of an embedded proof, the credential MUST append the proof in the `proof` property.
    /// </summary>
    [JsonConverter(typeof(AchievementCredentialConverter))]
    public partial class AchievementCredential : VerifiableCredential<AchievementSubject, Profile>
    {
        /// <summary>
        /// Timestamp of when the credential was awarded. `validFrom` is used to determine the most recent version of a Credential in conjunction with `issuer` and `id`. Consequently, the only way to update a Credental is to update the `validFrom`, losing the date when the Credential was originally awarded. `awardedDate` is meant to keep this original date.
        /// </summary>
        [Display(Description = "Timestamp of when the credential was awarded. `validFrom` is used to determine the most recent version of a Credential in conjunction with `issuer` and `id`. Consequently, the only way to update a Credental is to update the `validFrom`, losing the date when the Credential was originally awarded. `awardedDate` is meant to keep this original date.")]
        [JsonPropertyName("awardedDate")]
        [JsonPropertyOrder(18)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTimeOffset? AwardedDate { get; set; }

        /// <summary>
        /// <inheritdoc cref="VerifiableCredential{TSubject}.Context"/>
        /// </summary>
        /// <remarks>
        /// Within AchievementCredential, the value of the `@context` property MUST be an ordered set where the first two items are the URLs 'https://www.w3.org/ns/credentials/v2', 'https://purl.imsglobal.org/spec/ob/v3p0/context-3.0.3.json'.
        /// </remarks>
        [JsonPropertyName("@context")]
        [JsonConverter(typeof(JsonLdContextConverter))]
        [JsonPropertyOrder(1)]
        [Required]
        [MinLength(2)]
        public override ICollection<object> Context { get; set; } = new Collection<object>();

        private ICollection<EndorsementCredential> _endorsement = new Collection<EndorsementCredential>();
        /// <summary>
        /// Allows endorsers to make specific claims about the credential, and the achievement and profiles in the credential. These endorsements are signed with a Data Integrity proof format.
        /// </summary>
        [Display(Description = "Allows endorsers to make specific claims about the credential, and the achievement and profiles in the credential. These endorsements are signed with a Data Integrity proof format.")]
        [JsonPropertyName("endorsement")]
        [JsonConverter(typeof(EndorsementConverter))]
        [JsonPropertyOrder(20)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ICollection<EndorsementCredential> Endorsement
        {
            get => _endorsement;
            set => _endorsement = value ?? new Collection<EndorsementCredential>();
        }

        private ICollection<string> _endorsementJwt = new Collection<string>();
        /// <summary>
        /// Allows endorsers to make specific claims about the credential, and the achievement and profiles in the credential. These endorsements are signed with the VC-JWT proof format.
        /// </summary>
        [Display(Description = "Allows endorsers to make specific claims about the credential, and the achievement and profiles in the credential. These endorsements are signed with the VC-JWT proof format.")]
        [JsonPropertyName("endorsementJwt")]
        [JsonConverter(typeof(StringCollectionConverter))]
        [JsonPropertyOrder(21)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ICollection<string> EndorsementJwt
        {
            get => _endorsementJwt;
            set => _endorsementJwt = value ?? new Collection<string>();
        }

        private AchievementSubject? _credentialSubject;
        /// <summary>
        /// Claims about the achievement earner. AchievementCredential has exactly one credentialSubject.
        /// </summary>
        [JsonPropertyName("credentialSubject")]
        [JsonPropertyOrder(20)]
        [JsonConverter(typeof(SingleAchievementSubjectConverter))]
        [Required]
        public new AchievementSubject CredentialSubject
        {
            get => _credentialSubject!;
            set
            {
                _credentialSubject = value;
                // Keep base collection in sync for compatibility
                base.CredentialSubject.Clear();
                if (value != null)
                {
                    base.CredentialSubject.Add(value);
                }
            }
        }

        /// <summary>
        /// The image representing the credential for display purposes in wallets.
        /// </summary>
        [Display(Description = "The image representing the credential for display purposes in wallets.")]
        [JsonPropertyName("image")]
        [JsonPropertyOrder(13)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Image? Image { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <remarks>
        /// Within AchievementCredential the value of the type property MUST be an unordered set. One of the items MUST be the term 'VerifiableCredential', and one of the items MUST be the term 'AchievementCredential' or the term 'OpenBadgeCredential'.
        /// </remarks>
        [JsonPropertyName("type")]
        [JsonPropertyOrder(2)]
        [Required]
        [MinLength(2)]
        [JsonConverter(typeof(StringCollectionConverter))]
        public override ICollection<string> Type { get; set; } = new Collection<string>();
    }
}