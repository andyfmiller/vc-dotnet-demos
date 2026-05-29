#nullable enable

using IssuerApp.Data.Models.OpenBadges;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace IssuerApp.Data.Models
{
    /// <summary>
    /// The Organization acts as the publisher/issuer of credentials.
    /// </summary>
    public class Organization
    {
        [Key]
        public int OrganizationKey { get; init; }

        public int ProfileKey { get; set; }

        /// <summary>
        /// The profile used to issue verifiable credentials. This profile contains the necessary information and configuration for the organization to issue credentials, such as the issuer's DID, public key, and other relevant details. It serves as the basis for credential issuance and ensures that the credentials issued by the organization are properly formatted and contain the required information.
        /// </summary>
        public virtual Profile? Profile { get; set; }

        /// <summary>
        /// Gets the display name for the organization, preferring Profile.Name over Profile.Id.
        /// </summary>
        [NotMapped]
        public string DisplayName => !string.IsNullOrEmpty(Profile?.Name) ? Profile.Name : Profile?.Id ?? string.Empty;

        /// <summary>
        /// The multibase-encoded Ed25519 public key (multicodec prefix 0xed01 + 32-byte key, base58btc with 'z' prefix).
        /// Exposed via the did:web DID document's verificationMethod.
        /// </summary>
        public string? SigningPublicKeyMultibase { get; set; }

        /// <summary>
        /// The base64-encoded raw 32-byte Ed25519 private key seed.
        /// Used by the issuer to sign credentials. Keep this value secret.
        /// </summary>
        public string? SigningPrivateKeyBase64 { get; set; }

        /// <summary>
        /// The list of members that exist within this organization. This collection represents the individuals who are receiving credentials from the organization. It allows for tracking the members and their credentials within the system.
        /// </summary>
        [JsonIgnore]
        public virtual List<Member> Members { get; set; } = [];

        /// <summary>
        /// The list of achievements defined by this organization. This collection represents the achievements that can be issued as credentials to members within the organization.
        /// </summary>
        [JsonIgnore]
        public virtual List<Achievement> Achievements { get; set; } = [];

        /// <summary>
        /// The list of achievement credentials issued by this organization. This collection represents all verifiable credentials issued by the organization to members.
        /// </summary>
        [JsonIgnore]
        public virtual List<AchievementCredential> AchievementCredentials { get; set; } = [];
    }
}
