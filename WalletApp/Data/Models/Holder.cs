using System.ComponentModel.DataAnnotations;

namespace WalletApp.Data.Models
{
    public class Holder
    {
        [Key]
        public int? HolderKey { get; set; }

        /// <summary>
        /// The did:web identifier for this holder.
        /// Format: did:web:{encodedHost}:holders:{slug}
        /// </summary>
        [Required]
        public string Id { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The multibase-encoded Ed25519 public key (multicodec prefix 0xed01 + 32-byte key, base58btc with 'z' prefix).
        /// Exposed via the did:web DID document's verificationMethod.
        /// </summary>
        public string? SigningPublicKeyMultibase { get; set; }

        /// <summary>
        /// The base64-encoded raw 32-byte Ed25519 private key seed.
        /// Used by WalletApp to sign DIDAuthentication Verifiable Presentations.
        /// </summary>
        public string? SigningPrivateKeyBase64 { get; set; }
    }
}
