namespace Library.Models.OpenBadges
{
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json.Serialization;

    /// <summary>
    /// The IdentifierEntry complex type.
    /// </summary>
    public partial class IdentifierEntry
    {
        /// <summary>
        /// An identifier.
        /// </summary>
        [Display(Description = "An identifier.")]
        [JsonPropertyName("identifier")]
        [Required]
        public required string Identifier { get; set; }

        /// <summary>
        /// The identifier type.
        /// </summary>
        [Display(Description = "The identifier type.")]
        [JsonPropertyName("identifierType")]
        [Required]
        public required IdentifierType IdentifierType { get; set; }

        /// <summary>
        /// MUST be the IRI 'IdentifierEntry'.
        /// </summary>
        [Display(Description = "MUST be the IRI 'IdentifierEntry'.")]
        [JsonPropertyName("type")]
        [Required]
        public required string Type { get; set; }
    }
}