using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WalletApp.Data.Models
{
    public class HolderCredential
    {
        [Key]
        public int HolderCredentialKey { get; set; }

        [ForeignKey(nameof(Holder))]
        public int? HolderKey { get; set; }

        public virtual Holder? Holder { get; set; }

        /// <summary>
        /// The raw JSON of the VerifiableCredential as received from the issuer.
        /// </summary>
        public string CredentialJson { get; set; } = string.Empty;

        /// <summary>
        /// The value of the VerifiableCredential's "id" property, if present.
        /// </summary>
        public string? CredentialId { get; set; }

        public DateTimeOffset ReceivedAt { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Set when the credential is replaced by a newer version from the issuer
        /// (same <see cref="CredentialId"/>, new content).
        /// </summary>
        public DateTimeOffset? ReplacedAt { get; set; }

        /// <summary>
        /// The credential JSON immediately before it was last replaced, kept so the
        /// wallet can show the holder what changed.
        /// </summary>
        public string? PreviousCredentialJson { get; set; }
    }
}
