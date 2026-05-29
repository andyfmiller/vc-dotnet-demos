using System.Text.Json.Serialization;

namespace Library.DidWeb
{
    /// <summary>
    /// A verification method in a DID document.
    /// https://www.w3.org/TR/did-core/#verification-methods
    /// </summary>
    public class VerificationMethod
    {
        /// <summary>
        /// The unique identifier for this verification method, typically the DID with a fragment.
        /// </summary>
        [JsonPropertyName("id")]
        public required string Id { get; init; }

        /// <summary>
        /// The type of verification method (e.g. "JsonWebKey2020", "Ed25519VerificationKey2020").
        /// </summary>
        [JsonPropertyName("type")]
        public required string Type { get; init; }

        /// <summary>
        /// The DID of the controller of this verification method.
        /// </summary>
        [JsonPropertyName("controller")]
        public required string Controller { get; init; }

        /// <summary>
        /// The multibase-encoded public key material.
        /// For Ed25519VerificationKey2020 this is the multicodec-prefixed (0xed01)
        /// raw public key bytes encoded as base58btc with a 'z' prefix.
        /// </summary>
        [JsonPropertyName("publicKeyMultibase")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? PublicKeyMultibase { get; init; }
    }
}

