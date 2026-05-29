namespace Library.Models.OpenBadges
{
    using Library.Models.Converters;
    using Library.Models.OpenBadges.Converters;
    using Library.Models.Vc;
    using Library.Models.Vc.Converters;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json.Serialization;

    /// <summary>
    /// A verifiable credential that asserts a claim about an entity. As described in [[[#data-integrity]]], at least one proof mechanism, and the details necessary to evaluate that proof, MUST be expressed for a credential to be a verifiable credential. In the case of an embedded proof, the credential MUST append the proof in the `proof` property.
    /// </summary>
    [JsonConverter(typeof(EndorsementCredentialConverter))]
    public partial class EndorsementCredential
    {
        /// <summary>
        /// Timestamp of when the credential was awarded. `validFrom` is used to determine the most recent version of a Credential in conjunction with `issuer` and `id`. Consequently, the only way to update a Credental is to update the `validFrom`, losing the date when the Credential was originally awarded. `awardedDate` is meant to keep this original date.
        /// </summary>
        [Display(Description = "Timestamp of when the credential was awarded. `validFrom` is used to determine the most recent version of a Credential in conjunction with `issuer` and `id`. Consequently, the only way to update a Credental is to update the `validFrom`, losing the date when the Credential was originally awarded. `awardedDate` is meant to keep this original date.")]
        [JsonPropertyName("awardedDate")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTimeOffset? AwardedDate { get; set; }

        /// <summary>
        /// The value of the `@context` property MUST be an ordered set where the first item is a URI with the value 'https://www.w3.org/ns/credentials/v2', and the second item is a URI with the value 'https://purl.imsglobal.org/spec/ob/v3p0/context-3.0.3.json'.
        /// </summary>
        [Display(Description = "The value of the `@context` property MUST be an ordered set where the first item is a URI with the value 'https://www.w3.org/ns/credentials/v2', and the second item is a URI with the value 'https://purl.imsglobal.org/spec/ob/v3p0/context-3.0.3.json'.")]
        [JsonPropertyName("@context")]
        [Required]
        [MinLength(1)]
        [JsonConverter(typeof(JsonLdContextConverter))]
        public ICollection<object> Context { get; set; } = new Collection<object>();

        private ICollection<CredentialSchema> _credentialSchema = new Collection<CredentialSchema>();
        /// <summary>
        /// The value of the `credentialSchema` property MUST be one or more data schemas that provide verifiers with enough information to determine if the provided data conforms to the provided schema.
        /// </summary>
        [Display(Description = "The value of the `credentialSchema` property MUST be one or more data schemas that provide verifiers with enough information to determine if the provided data conforms to the provided schema.")]
        [JsonPropertyName("credentialSchema")]
        [JsonConverter(typeof(CredentialSchemaConverter))]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ICollection<CredentialSchema>? CredentialSchema
        {
            get => _credentialSchema;
            set => _credentialSchema = value ?? new Collection<CredentialSchema>();
        }

        /// <summary>
        /// The credential status information.
        /// </summary>
        [Display(Description = "The credential status information.")]
        [JsonPropertyName("credentialStatus")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public CredentialStatus? CredentialStatus { get; set; }

        /// <summary>
        /// The endorsement subject being asserted.
        /// </summary>
        [Display(Description = "The endorsement subject being asserted.")]
        [JsonPropertyName("credentialSubject")]
        [Required]
        public required EndorsementSubject CredentialSubject { get; set; }

        /// <summary>
        /// The short description of the credential for display purposes in wallets.
        /// </summary>
        [Display(Description = "The short description of the credential for display purposes in wallets.")]
        [JsonPropertyName("description")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description { get; set; }

        private ICollection<Evidence> _evidence = new Collection<Evidence>();
        /// <summary>
        /// A description of the work that the recipient did to earn the credential. This can be a page that links out to other pages if linking directly to the work is infeasible.
        /// </summary>
        [Display(Description = "A description of the work that the recipient did to earn the credential. This can be a page that links out to other pages if linking directly to the work is infeasible.")]
        [JsonPropertyName("evidence")]
        [JsonConverter(typeof(EvidenceConverter))]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ICollection<Evidence>? Evidence
        {
            get => _evidence;
            set => _evidence = value ?? new Collection<Evidence>();
        }

        /// <summary>
        /// Unambiguous reference to the credential.
        /// </summary>
        [Display(Description = "Unambiguous reference to the credential.")]
        [JsonPropertyName("id")]
        [Required]
        public required string Id { get; set; }

        /// <summary>
        /// A description of the individual, entity, or organization that issued the credential.
        /// </summary>
        [Display(Description = "A description of the individual, entity, or organization that issued the credential.")]
        [JsonPropertyName("issuer")]
        [Required]
        public required Profile Issuer { get; set; }

        /// <summary>
        /// The name of the credential for display purposes in wallets. For example, in a list of credentials and in detail views.
        /// </summary>
        [Display(Description = "The name of the credential for display purposes in wallets. For example, in a list of credentials and in detail views.")]
        [JsonPropertyName("name")]
        [Required]
        public required string Name { get; set; }

        private ICollection<DataIntegrityProof> _proof = new Collection<DataIntegrityProof>();
        /// <summary>
        /// If present, one or more embedded cryptographic proofs that can be used to detect tampering and verify the authorship of the credential.
        /// </summary>
        [Display(Description = "If present, one or more embedded cryptographic proofs that can be used to detect tampering and verify the authorship of the credential.")]
        [JsonPropertyName("proof")]
        [JsonConverter(typeof(ProofConverter))]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ICollection<DataIntegrityProof>? Proof
        {
            get => _proof;
            set => _proof = value ?? new Collection<DataIntegrityProof>();
        }

        /// <summary>
        /// The refresh service information.
        /// </summary>
        [Display(Description = "The refresh service information.")]
        [JsonPropertyName("refreshService")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public RefreshService? RefreshService { get; set; }

        private ICollection<TermsOfUse> _termsOfUse = new Collection<TermsOfUse>();
        /// <summary>
        /// The value of the `termsOfUse` property tells the verifier what actions it is required to perform (an obligation), not allowed to perform (a prohibition), or allowed to perform (a permission) if it is to accept the verifiable credential.
        /// </summary>
        [Display(Description = "The value of the `termsOfUse` property tells the verifier what actions it is required to perform (an obligation), not allowed to perform (a prohibition), or allowed to perform (a permission) if it is to accept the verifiable credential.")]
        [JsonPropertyName("termsOfUse")]
        [JsonConverter(typeof(TermsOfUseConverter))]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ICollection<TermsOfUse>? TermsOfUse
        {
            get => _termsOfUse;
            set => _termsOfUse = value ?? new Collection<TermsOfUse>();
        }

        /// <summary>
        /// The value of the type property MUST be an unordered set. One of the items MUST be the URI 'VerifiableCredential', and one of the items MUST be the URI 'EndorsementCredential'.
        /// </summary>
        [Display(Description = "The value of the type property MUST be an unordered set. One of the items MUST be the URI 'VerifiableCredential', and one of the items MUST be the URI 'EndorsementCredential'.")]
        [JsonPropertyName("type")]
        [Required]
        [MinLength(1)]
        [JsonConverter(typeof(StringCollectionConverter))]
        public ICollection<string> Type { get; set; } = new Collection<string>();

        /// <summary>
        /// Timestamp of when the credential becomes valid.
        /// </summary>
        [Display(Description = "Timestamp of when the credential becomes valid.")]
        [JsonPropertyName("validFrom")]
        [Required]
        public DateTimeOffset ValidFrom { get; set; }

        /// <summary>
        /// If the credential has some notion of validity period, this indicates a timestamp when a credential should no longer be considered valid. After this time, the credential should be considered invalid.
        /// </summary>
        [Display(Description = "If the credential has some notion of validity period, this indicates a timestamp when a credential should no longer be considered valid. After this time, the credential should be considered invalid.")]
        [JsonPropertyName("validUntil")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTimeOffset? ValidUntil { get; set; }

        private Dictionary<string, object>? _additionalProperties;

        /// <summary>
        /// Additional properties not defined in the schema.
        /// </summary>
        [JsonExtensionData]
        [JsonPropertyName("additionalProperties")]
        [Display(Name = "Additional Properties", Description = "Additional properties not defined in the schema.")]
        public Dictionary<string, object> AdditionalProperties
        {
            get { return _additionalProperties ??= []; }
            set { _additionalProperties = value; }
        }
    }
}