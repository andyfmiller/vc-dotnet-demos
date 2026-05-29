namespace Library.Models.Vc
{
    using Library.Models.Vc.Converters;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json.Serialization;

    /// <summary>
    /// A data integrity proof provides information about the proof mechanism, parameters required to verify that proof, and the proof value itself. All of this information is provided using Linked Data vocabularies such as The Security Vocabulary.
    /// </summary>
    /// <example>
    /// {
    ///   "myWebsite": "https://hello.world.example/",
    ///   "proof": {
    ///     "type": "DataIntegrityProof",
    ///     "cryptosuite": "eddsa-jcs-2022",
    ///     "created": "2023-03-05T19:23:24Z",
    ///     "verificationMethod": "https://di.example/issuer#z6MkjLrk3gKS2nnkeWcmcxiZPGskmesDpuwRBorgHxUXfxnG",
    ///     "proofPurpose": "assertionMethod",
    ///     "proofValue": "zQeVbY4oey5q2M3XKaxup3tmzN4DRFTLVqpLMweBrSxMY2xHX5XTYV8nQApmEcqaqA3Q1gVHMrXFkXJeV6doDwLWx"
    ///   }
    /// }
    /// </example>
    public partial class DataIntegrityProof
    {
        /// <summary>
        /// An optional identifier for the proof, which MUST be a URL [URL], such as a UUID as a URN (urn:uuid:6a1676b8-b51f-11ed-937b-d76685a20ff5).
        /// </summary>
        [Display(Description = "An optional identifier for the proof, which MUST be a URL [URL], such as a UUID as a URN (urn:uuid:6a1676b8-b51f-11ed-937b-d76685a20ff5).")]
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        /// <summary>
        /// The specific type of proof MUST be specified as a string that maps to a URL [URL]. Examples of proof types include DataIntegrityProof and Ed25519Signature2020. Proof types determine what other fields are required to secure and verify the proof.
        /// </summary>
        [Display(Description = "The specific type of proof MUST be specified as a string that maps to a URL [URL]. Examples of proof types include DataIntegrityProof and Ed25519Signature2020. Proof types determine what other fields are required to secure and verify the proof.")]
        [JsonPropertyName("type")]
        [Required]
        public required string Type { get; set; }

        /// <summary>
        /// The reason the proof was created MUST be specified as a string that maps to a URL [URL]. The proof purpose acts as a safeguard to prevent the proof from being misused by being applied to a purpose other than the one that was intended. For example, without this value the creator of a proof could be tricked into using cryptographic material typically used to create a Verifiable Credential (assertionMethod) during a login process (authentication) which would then result in the creation of a verifiable credential they never meant to create instead of the intended action, which was to merely log in to a website.
        /// </summary>
        [Display(Description = "The reason the proof was created MUST be specified as a string that maps to a URL [URL]. The proof purpose acts as a safeguard to prevent the proof from being misused by being applied to a purpose other than the one that was intended. For example, without this value the creator of a proof could be tricked into using cryptographic material typically used to create a Verifiable Credential (assertionMethod) during a login process (authentication) which would then result in the creation of a verifiable credential they never meant to create instead of the intended action, which was to merely log in to a website.")]
        [JsonPropertyName("proofPurpose")]
        [Required]
        public required string ProofPurpose { get; set; }

        /// <summary>
        /// A verification method is the means and information needed to verify the proof. If included, the value MUST be a string that maps to a [URL]. Inclusion of verificationMethod is OPTIONAL, but if it is not included, other properties such as cryptosuite might provide a mechanism by which to obtain the information necessary to verify the proof. Note that when verificationMethod is expressed in a data integrity proof, the value points to the actual location of the data; that is, the verificationMethod references, via a URL, the location of the public key that can be used to verify the proof. This public key data is stored in a controlled identifier document, which contains a full description of the verification method.
        /// </summary>
        [Display(Description = "A verification method is the means and information needed to verify the proof. If included, the value MUST be a string that maps to a [URL]. Inclusion of verificationMethod is OPTIONAL, but if it is not included, other properties such as cryptosuite might provide a mechanism by which to obtain the information necessary to verify the proof. Note that when verificationMethod is expressed in a data integrity proof, the value points to the actual location of the data; that is, the verificationMethod references, via a URL, the location of the public key that can be used to verify the proof. This public key data is stored in a controlled identifier document, which contains a full description of the verification method.")]
        [JsonPropertyName("verificationMethod")]
        public string? VerificationMethod { get; set; }
        
        /// <summary>
        /// An identifier for the cryptographic suite that can be used to verify the proof. See 3. Cryptographic Suites for more information. If the proof type is DataIntegrityProof, cryptosuite MUST be specified; otherwise, cryptosuite MAY be specified. If specified, its value MUST be a string.
        /// </summary>
        [Display(Description = "An identifier for the cryptographic suite that can be used to verify the proof. See 3. Cryptographic Suites for more information. If the proof type is DataIntegrityProof, cryptosuite MUST be specified; otherwise, cryptosuite MAY be specified. If specified, its value MUST be a string.")]
        [JsonPropertyName("cryptosuite")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Cryptosuite { get; set; }

        /// <summary>
        /// The date and time the proof was created is OPTIONAL and, if included, MUST be specified as an [XMLSCHEMA11-2] dateTimeStamp string, either in Universal Coordinated Time (UTC), denoted by a Z at the end of the value, or with a time zone offset relative to UTC. A conforming processor MAY chose to consume time values that were incorrectly serialized without an offset. Incorrectly serialized time values without an offset are to be interpreted as UTC.
        /// </summary>
        [Display(Description = "The date and time the proof was created is OPTIONAL and, if included, MUST be specified as an [XMLSCHEMA11-2] dateTimeStamp string, either in Universal Coordinated Time (UTC), denoted by a Z at the end of the value, or with a time zone offset relative to UTC. A conforming processor MAY chose to consume time values that were incorrectly serialized without an offset. Incorrectly serialized time values without an offset are to be interpreted as UTC.")]
        [JsonPropertyName("created")]
        [JsonConverter(typeof(NullableDateTimeOffsetUtcConverter))]
        public DateTimeOffset? Created { get; set; }

        /// <summary>
        /// The expires property is OPTIONAL and, if present, specifies when the proof expires. If present, it MUST be an [XMLSCHEMA11-2] dateTimeStamp string, either in Universal Coordinated Time (UTC), denoted by a Z at the end of the value, or with a time zone offset relative to UTC. A conforming processor MAY chose to consume time values that were incorrectly serialized without an offset. Incorrectly serialized time values without an offset are to be interpreted as UTC.
        /// </summary>
        [Display(Description = "The expires property is OPTIONAL and, if present, specifies when the proof expires. If present, it MUST be an [XMLSCHEMA11-2] dateTimeStamp string, either in Universal Coordinated Time (UTC), denoted by a Z at the end of the value, or with a time zone offset relative to UTC. A conforming processor MAY chose to consume time values that were incorrectly serialized without an offset. Incorrectly serialized time values without an offset are to be interpreted as UTC.")]
        [JsonPropertyName("expires")]
        [JsonConverter(typeof(NullableDateTimeOffsetUtcConverter))]
        public DateTimeOffset? Expires { get; set; }

        /// <summary>
        /// The domain property is OPTIONAL. It conveys one or more security domains in which the proof is meant to be used. If specified, the associated value MUST be either a string, or an unordered set of strings. A verifier SHOULD use the value to ensure that the proof was intended to be used in the security domain in which the verifier is operating. The specification of the domain parameter is useful in challenge-response protocols where the verifier is operating from within a security domain known to the creator of the proof. Example domain values include: domain.example (DNS domain), https://domain.example:8443 (Web origin), mycorp-intranet (bespoke text string), and b31d37d4-dd59-47d3-9dd8-c973da43b63a (UUID).
        /// </summary>
        [Display(Description = "The domain property is OPTIONAL. It conveys one or more security domains in which the proof is meant to be used. If specified, the associated value MUST be either a string, or an unordered set of strings. A verifier SHOULD use the value to ensure that the proof was intended to be used in the security domain in which the verifier is operating. The specification of the domain parameter is useful in challenge-response protocols where the verifier is operating from within a security domain known to the creator of the proof. Example domain values include: domain.example (DNS domain), https://domain.example:8443 (Web origin), mycorp-intranet (bespoke text string), and b31d37d4-dd59-47d3-9dd8-c973da43b63a (UUID).")]
        [JsonPropertyName("domain")]
        public string? Domain { get; set; }

        /// <summary>
        /// A string value that SHOULD be included in a proof if a domain is specified. The value is used once for a particular domain and window of time. This value is used to mitigate replay attacks. Examples of a challenge value include: 1235abcd6789, 79d34551-ae81-44ae-823b-6dadbab9ebd4, and ruby.
        /// </summary>
        [Display(Description = "A string value that SHOULD be included in a proof if a domain is specified. The value is used once for a particular domain and window of time. This value is used to mitigate replay attacks. Examples of a challenge value include: 1235abcd6789, 79d34551-ae81-44ae-823b-6dadbab9ebd4, and ruby.")]
        [JsonPropertyName("challenge")]
        public string? Challenge { get; set; }

        /// <summary>
        /// A string value that expresses base-encoded binary data necessary to verify the digital proof using the verificationMethod specified. The value MUST use a header and encoding as described in Section 2.4 Multibase of the Controlled Identifiers v1.0 specification to express the binary data. The contents of this value are determined by a specific cryptosuite and set to the proof value generated by the Add Proof Algorithm for that cryptosuite. Alternative properties with different encodings specified by the cryptosuite MAY be used, instead of this property, to encode the data necessary to verify the digital proof.
        /// </summary>
        [Display(Description = "A string value that expresses base-encoded binary data necessary to verify the digital proof using the verificationMethod specified. The value MUST use a header and encoding as described in Section 2.4 Multibase of the Controlled Identifiers v1.0 specification to express the binary data. The contents of this value are determined by a specific cryptosuite and set to the proof value generated by the Add Proof Algorithm for that cryptosuite. Alternative properties with different encodings specified by the cryptosuite MAY be used, instead of this property, to encode the data necessary to verify the digital proof.")]
        [JsonPropertyName("proofValue")]
        public string? ProofValue { get; set; }

        /// <summary>
        /// The previousProof property is OPTIONAL. If present, it MUST be a string value or an unordered list of string values. Each value identifies another data integrity proof, all of which MUST also verify for the current proof to be considered verified. This property is used in Section 2.1.2 Proof Chains.
        /// </summary>
        [Display(Description = "The previousProof property is OPTIONAL. If present, it MUST be a string value or an unordered list of string values. Each value identifies another data integrity proof, all of which MUST also verify for the current proof to be considered verified. This property is used in Section 2.1.2 Proof Chains.")]
        [JsonPropertyName("previousProof")]
        public string? PreviousProof { get; set; }

        /// <summary>
        /// An OPTIONAL string value supplied by the proof creator. One use of this field is to increase privacy by decreasing linkability that is the result of deterministically generated signatures.
        /// </summary>
        [Display(Description = "An OPTIONAL string value supplied by the proof creator. One use of this field is to increase privacy by decreasing linkability that is the result of deterministically generated signatures.")]
        [JsonPropertyName("nonce")]
        public string? Nonce { get; set; }

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