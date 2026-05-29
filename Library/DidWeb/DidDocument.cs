using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Library.DidWeb
{
    /// <summary>
    /// Represents a DID document conforming to the W3C DID Core specification.
    /// https://www.w3.org/TR/did-core/
    /// </summary>
    public class DidDocument
    {
        [JsonPropertyName("@context")]
        public List<string> Context { get; init; } = ["https://www.w3.org/ns/did/v1"];

        /// <summary>
        /// The DID that this document describes.
        /// </summary>
        [JsonPropertyName("id")]
        public required string Id { get; init; }

        /// <summary>
        /// Verification methods associated with this DID.
        /// </summary>
        [JsonPropertyName("verificationMethod")]
        public List<VerificationMethod> VerificationMethod { get; init; } = [];

        /// <summary>
        /// Verification method references used for authentication.
        /// </summary>
        [JsonPropertyName("authentication")]
        public List<string> Authentication { get; init; } = [];

        /// <summary>
        /// Verification method references used for assertion (e.g. signing credentials).
        /// </summary>
        [JsonPropertyName("assertionMethod")]
        public List<string> AssertionMethod { get; init; } = [];
    }
}
