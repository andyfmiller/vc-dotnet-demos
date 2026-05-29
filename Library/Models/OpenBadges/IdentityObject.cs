namespace Library.Models.OpenBadges
{
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json.Serialization;

    /// <summary>
    /// A collection of information about the recipient of an achievement.
    /// </summary>
    public partial class IdentityObject
    {
        /// <summary>
        /// If the `identityHash` is hashed, this should contain the string used to salt the hash. If this value is not provided, it should be assumed that the hash was not salted.
        /// </summary>
        [Display(Description = "If the `identityHash` is hashed, this should contain the string used to salt the hash. If this value is not provided, it should be assumed that the hash was not salted.")]
        [Obsolete("The 'salt' property is deprecated and should not be used. Using a hash of the identity value is no longer recommended.")]
        [JsonPropertyName("salt")]
        public string? Salt { get; set; }

        /// <summary>
        /// Either the IdentityHash of the identity or the plaintext value. If it's possible that the plaintext transmission and storage of the identity value would leak personally identifiable information where there is an expectation of privacy, it is strongly recommended that an IdentityHash be used.
        /// </summary>
        /// <remarks>
        /// Using a hash of the identity value is no longer recommended. It is recommended to use the plaintext value of the identity.
        /// </remarks>
        [Display(Description = "Either the IdentityHash of the identity or the plaintext value. If it's possible that the plaintext transmission and storage of the identity value would leak personally identifiable information where there is an expectation of privacy, it is strongly recommended that an IdentityHash be used.")]
        [JsonPropertyName("identityHash")]
        [Required]
        public required string IdentityHash { get; set; }

        /// <summary>
        /// The identity type.
        /// </summary>
        [Display(Description = "The identity type.")]
        [JsonPropertyName("identityType")]
        [Required]
        public required IdentifierType IdentityType { get; set; }

        /// <summary>
        /// Whether or not the `identityHash` value is hashed.
        /// </summary>
        [Display(Description = "Whether or not the `identityHash` value is hashed.")]
        [Obsolete("The 'hashed' property is deprecated and should not be used. Using a hash of the identity value is no longer recommended.")]
        [JsonPropertyName("hashed")]
        [Required]
        public bool Hashed { get; set; }

        /// <summary>
        /// MUST be the term 'IdentityObject'.
        /// </summary>
        [Display(Description = "MUST be the term 'IdentityObject'.")]
        [JsonPropertyName("type")]
        [Required]
        public required string Type { get; set; }

    }

}