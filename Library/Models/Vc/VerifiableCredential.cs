namespace Library.Models.Vc
{
    using Library.Models.Converters;
    using Library.Models.Vc.Converters;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Text.Json.Serialization;

    /// <summary>
    /// A Verifiable Credential as defined in the [[VC-DATA-MODEL-2.0]]. As described in [[[#proofs-signatures]]], at least one proof mechanism, and the details necessary to evaluate that proof, MUST be expressed for a credential to be a verifiable credential. In the case of an embedded proof, the credential MUST append the proof in the `proof` property.
    /// </summary>
    public partial class VerifiableCredential<TSubject, TIssuer> 
        where TSubject : CredentialSubject 
        where TIssuer : Issuer
    {
        /// <summary>
        /// The value of the `@context` property MUST be an ordered set where the first item is a URI with the value 'https://www.w3.org/ns/credentials/v2'.
        /// Subsequent items in the ordered set MUST be composed of any combination of URLs and objects, where each is processable as a JSON-LD Context.
        /// </summary>
        [Display(Description = "The value of the `@context` property MUST be an ordered set where the first item is a URI with the value 'https://www.w3.org/ns/credentials/v2'. Subsequent items in the ordered set MUST be composed of any combination of URLs and objects, where each is processable as a JSON-LD Context.")]
        [JsonPropertyName("@context")]
        [JsonPropertyOrder(1)]
        [JsonConverter(typeof(JsonLdContextConverter))]
        [Required]
        [MinLength(1)]
        public virtual ICollection<object> Context { get; set; } = new Collection<object>();

        /// <summary>
        /// Unambiguous reference to the credential.
        /// </summary>
        [Display(Description = "Unambiguous reference to the credential.")]
        [JsonPropertyName("id")]
        [JsonPropertyOrder(3)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Id { get; set; }

        /// <summary>
        /// The value of the type property MUST be an unordered set. One of the items MUST be the term 'VerifiableCredential'.
        /// </summary>
        [Display(Description = "The value of the type property MUST be an unordered set. One of the items MUST be the term 'VerifiableCredential'.")]
        [JsonPropertyName("type")]
        [JsonPropertyOrder(2)]
        [Required]
        [MinLength(1)]
        [JsonConverter(typeof(StringCollectionConverter))]
        public virtual ICollection<string> Type { get; set; } = new Collection<string>();

        /// <summary>
        /// A description of the individual, entity, or organization that issued the credential.
        /// </summary>
        [Display(Description = "A description of the individual, entity, or organization that issued the credential.")]
        [JsonPropertyName("issuer")]
        [JsonPropertyOrder(4)]
        [JsonConverter(typeof(RequiredIssuerConverter))]
        [Required]
        public virtual required TIssuer Issuer { get; set; }

        /// <summary>
        /// Timestamp of when the credential becomes valid.
        /// </summary>
        [Display(Description = "Timestamp of when the credential becomes valid.")]
        [JsonPropertyName("validFrom")]
        [JsonPropertyOrder(5)]
        [Required]
        [JsonConverter(typeof(DateTimeOffsetUtcConverter))]
        public DateTimeOffset ValidFrom { get; set; }

        /// <summary>
        /// If the credential has some notion of validity period, this indicates a timestamp when a credential should no longer be considered valid. After this time, the credential should be considered invalid.
        /// </summary>
        [Display(Description = "If the credential has some notion of validity period, this indicates a timestamp when a credential should no longer be considered valid. After this time, the credential should be considered invalid.")]
        [JsonPropertyName("validUntil")]
        [JsonPropertyOrder(8)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTimeOffset? ValidUntil { get; set; }

        /// <summary>
        /// The value of the credentialSubject property is a set of objects where each object MUST be the subject of one or more claims, which MUST be serialized inside the credentialSubject property.
        /// Each object MAY contain an id property. Can be a single object or an array of objects.
        /// </summary>
        [Display(Description = "The value of the credentialSubject property is a set of objects where each object MUST be the subject of one or more claims, which MUST be serialized inside the credentialSubject property. Each object MAY contain an id property. Can be a single object or an array of objects.")]
        [JsonPropertyName("credentialSubject")]
        [JsonPropertyOrder(20)]
        [JsonConverter(typeof(CredentialSubjectConverterFactory))]
        [Required]
        [MinLength(1)]
        public ICollection<TSubject> CredentialSubject { get; set; } = new Collection<TSubject>();

        private ICollection<CredentialSchema> _credentialSchema = new Collection<CredentialSchema>();
        /// <summary>
        /// Data schemas are useful when enforcing a specific structure on a given data collection. There are at least two types of data schemas that this specification considers:
        /// Data verification schemas, which are used to establish that the structure and contents of a credential or verifiable credential conform to a published schema.
        /// Data encoding schemas, which are used to map the contents of a verifiable credential to an alternative representation format, such as a format used in a zero-knowledge proof.
        /// If specified, the value of the `credentialSchema` property MUST be one or more data schemas that provide verifiers with enough information to determine if the provided data conforms to the provided schema.
        /// </summary>
        [Display(Description = "Data schemas are useful when enforcing a specific structure on a given data collection. There are at least two types of data schemas that this specification considers: Data verification schemas, which are used to establish that the structure and contents of a credential or verifiable credential conform to a published schema. Data encoding schemas, which are used to map the contents of a verifiable credential to an alternative representation format, such as a format used in a zero-knowledge proof. If specified, the value of the `credentialSchema` property MUST be one or more data schemas that provide verifiers with enough information to determine if the provided data conforms to the provided schema.")]
        [JsonPropertyName("credentialSchema")]
        [JsonPropertyOrder(8)]
        [JsonConverter(typeof(CredentialSchemaConverter))]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ICollection<CredentialSchema>? CredentialSchema
        { 
            get => _credentialSchema.Count == 0 ? null : _credentialSchema;
            set => _credentialSchema = value ?? new Collection<CredentialSchema>();
        }

        private ICollection<CredentialStatus> _credentialStatus = new Collection<CredentialStatus>();
        /// <summary>
        /// The 'credentialStatus' property describes the mechanism used to determine the status of a credential. 
        /// This is used when a credential has some notion of being revoked, suspended, or expired, and the issuer 
        /// wishes to provide a mechanism for verifiers to check the status of a credential.
        /// Can be a single object or an array of objects.
        /// </summary>
        [Display(Description = "The 'credentialStatus' property describes the mechanism used to determine the status of a credential. This is used when a credential has some notion of being revoked, suspended, or expired, and the issuer wishes to provide a mechanism for verifiers to check the status of a credential. Can be a single object or an array of objects.")]
        [JsonPropertyName("credentialStatus")]
        [JsonPropertyOrder(9)]
        [JsonConverter(typeof(CredentialStatusConverter))]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ICollection<CredentialStatus>? CredentialStatus
        { 
            get => _credentialStatus.Count == 0 ? null : _credentialStatus;
            set => _credentialStatus = value ?? new Collection<CredentialStatus>();
        }

        /// <summary>
        /// An OPTIONAL property that conveys specific details about a credential. If present, the value of the description property MUST be a string or a language value object as described in 11.1 Language and Base Direction. Ideally, the description of a credential is no more than a few sentences in length and conveys enough information about the credential to remind an individual of its contents without having to look through the entirety of the claims.
        /// </summary>
        [Display(Description = "An OPTIONAL property that conveys specific details about a credential. If present, the value of the description property MUST be a string or a language value object as described in 11.1 Language and Base Direction. Ideally, the description of a credential is no more than a few sentences in length and conveys enough information about the credential to remind an individual of its contents without having to look through the entirety of the claims.")]
        [JsonPropertyName("description")]
        [JsonPropertyOrder(7)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description { get; set; }

        private ICollection<Evidence> _evidence = new Collection<Evidence>();
        /// <summary>
        /// Evidence can be included by an issuer to provide the verifier with additional supporting information in a verifiable credential. 
        /// This could be used by the verifier to establish the confidence with which it relies on the claims in the verifiable credential. 
        /// For example, an issuer could check physical documentation provided by the subject or perform a set of background checks before issuing the credential.
        /// If present, the value MUST be either a single object or a set of one or more objects.
        /// </summary>
        [Display(Description = "Evidence can be included by an issuer to provide the verifier with additional supporting information in a verifiable credential. This could be used by the verifier to establish the confidence with which it relies on the claims in the verifiable credential. For example, an issuer could check physical documentation provided by the subject or perform a set of background checks before issuing the credential. If present, the value MUST be either a single object or a set of one or more objects.")]
        [JsonPropertyName("evidence")]
        [JsonPropertyOrder(11)]
        [JsonConverter(typeof(EvidenceConverter))]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ICollection<Evidence>? Evidence
        { 
            get => _evidence.Count == 0 ? null : _evidence;
            set => _evidence = value ?? new Collection<Evidence>();
        }

        /// <summary>
        /// An OPTIONAL property that expresses the name of the credential. If present, the value of the name property MUST be a string or a language value object as described in 11.1 Language and Base Direction. Ideally, the name of a credential is concise, human-readable, and could enable an individual to quickly differentiate one credential from any other credentials they might hold.
        /// </summary>
        [Display(Description = "An OPTIONAL property that expresses the name of the credential. If present, the value of the name property MUST be a string or a language value object as described in 11.1 Language and Base Direction. Ideally, the name of a credential is concise, human-readable, and could enable an individual to quickly differentiate one credential from any other credentials they might hold.")]
        [JsonPropertyName("name")]
        [JsonPropertyOrder(6)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Name { get; set; }

        private ICollection<RefreshService> _refreshService = new Collection<RefreshService>();
        /// <summary>
        /// The information in RefreshService is used to refresh the verifiable credential.
        /// It is useful for systems to enable the manual or automatic refresh of an expired verifiable credential.
        /// The issuer can include the refresh service to enable either the holder or the verifier to perform future updates of the credential.
        /// If specified, the value of the refreshService property MUST be one or more refresh services that provides enough information to refresh the verifiable credential.
        /// </summary>
        [Display(Description = "The information in RefreshService is used to refresh the verifiable credential. It is useful for systems to enable the manual or automatic refresh of an expired verifiable credential. The issuer can include the refresh service to enable either the holder or the verifier to perform future updates of the credential. If specified, the value of the refreshService property MUST be one or more refresh services that provides enough information to refresh the verifiable credential.")]
        [JsonPropertyName("refreshService")]
        [JsonPropertyOrder(15)]
        [JsonConverter(typeof(RefreshServiceConverter))]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ICollection<RefreshService>? RefreshService
        { 
            get => _refreshService.Count == 0 ? null : _refreshService;
            set => _refreshService = value ?? new Collection<RefreshService>();
        }

        private ICollection<TermsOfUse> _termsOfUse = new Collection<TermsOfUse>();
        /// <summary>
        /// The value of the `termsOfUse` property tells the verifier what actions it is required to perform (an obligation), 
        /// not allowed to perform (a prohibition), or allowed to perform (a permission) if it is to accept the verifiable credential.
        /// Terms of use can be used by an issuer or a holder to communicate the terms under which a verifiable credential 
        /// or verifiable presentation was issued. If specified, the value MUST specify one or more terms of use policies 
        /// under which the creator issued the credential.
        /// </summary>
        [Display(Description = "The value of the `termsOfUse` property tells the verifier what actions it is required to perform (an obligation), not allowed to perform (a prohibition), or allowed to perform (a permission) if it is to accept the verifiable credential. Terms of use can be used by an issuer or a holder to communicate the terms under which a verifiable credential or verifiable presentation was issued. If specified, the value MUST specify one or more terms of use policies under which the creator issued the credential.")]
        [JsonPropertyName("termsOfUse")]
        [JsonPropertyOrder(16)]
        [JsonConverter(typeof(TermsOfUseConverter))]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ICollection<TermsOfUse>? TermsOfUse
        { 
            get => _termsOfUse.Count == 0 ? null : _termsOfUse;
            set => _termsOfUse = value ?? new Collection<TermsOfUse>();
        }

        /// <summary>
        /// A property used for specifying one or more methods that a verifier might use to increase their confidence that the value of a property in or of a verifiable credential or verifiable presentation is accurate. The associated vocabulary URL MUST be https://www.w3.org/2018/credentials#confidenceMethod.
        /// This is a possible extension point. Implementers MAY use these properties, but SHOULD expect them and/or their meanings to change during the process of normatively specifying them. Implementers SHOULD NOT use these properties without a publicly disclosed specification describing their implementation.
        /// </summary>
        [Display(Description = "A property used for specifying one or more methods that a verifier might use to increase their confidence that the value of a property in or of a verifiable credential or verifiable presentation is accurate. The associated vocabulary URL MUST be https://www.w3.org/2018/credentials#confidenceMethod. This is a possible extension point. Implementers MAY use these properties, but SHOULD expect them and/or their meanings to change during the process of normatively specifying them. Implementers SHOULD NOT use these properties without a publicly disclosed specification describing their implementation.")]
        [JsonPropertyName("confidenceMethod")]
        [JsonPropertyOrder(17)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? ConfidenceMethod { get; set; }

        /// <summary>
        /// A property used for specifying one or more methods to render a credential into a visual, auditory, haptic, or other format. The associated vocabulary URL MUST be https://www.w3.org/2018/credentials#renderMethod.
        /// This is a possible extension point. Implementers MAY use these properties, but SHOULD expect them and/or their meanings to change during the process of normatively specifying them. Implementers SHOULD NOT use these properties without a publicly disclosed specification describing their implementation.
        /// </summary>
        [Display(Description = "A property used for specifying one or more methods to render a credential into a visual, auditory, haptic, or other format. The associated vocabulary URL MUST be https://www.w3.org/2018/credentials#renderMethod. This is a possible extension point. Implementers MAY use these properties, but SHOULD expect them and/or their meanings to change during the process of normatively specifying them. Implementers SHOULD NOT use these properties without a publicly disclosed specification describing their implementation.")]
        [JsonPropertyName("renderMethod")]
        [JsonPropertyOrder(19)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? RenderMethod { get; set; }

        private ICollection<DataIntegrityProof> _proof = new Collection<DataIntegrityProof>();
        /// <summary>
        /// If present, one or more embedded cryptographic proofs that can be used to detect tampering and verify the authorship of the credential.
        /// </summary>
        [Display(Description = "If present, one or more embedded cryptographic proofs that can be used to detect tampering and verify the authorship of the credential.")]
        [JsonPropertyName("proof")]
        [JsonPropertyOrder(21)]
        [JsonConverter(typeof(ProofConverter))]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ICollection<DataIntegrityProof>? Proof
        {
            get => _proof.Count == 0 ? null : _proof;
            set => _proof = value ?? new Collection<DataIntegrityProof>();
        }

        private Dictionary<string, object>? _additionalProperties;

        /// <summary>
        /// Additional properties not defined in the schema.
        /// </summary>
        [JsonExtensionData]
        [JsonPropertyName("additionalProperties")]
        [JsonPropertyOrder(int.MaxValue)]
        [Display(Name = "Additional Properties", Description = "Additional properties not defined in the schema.")]
        public Dictionary<string, object> AdditionalProperties
        {
            get { return _additionalProperties ??= []; }
            set { _additionalProperties = value; }
        }

        /// <summary>
        /// Validates the credential context according to the rules.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the context is invalid.</exception>
        public void ValidateContext()
        {
            if (Context == null || Context.Count == 0)
                throw new InvalidOperationException("@context must contain at least one item.");

            var firstItem = Context.First();
            if (firstItem is not string firstUrl || firstUrl != "https://www.w3.org/ns/credentials/v2")
                throw new InvalidOperationException("First @context item must be 'https://www.w3.org/ns/credentials/v2'.");
        }
    }
}