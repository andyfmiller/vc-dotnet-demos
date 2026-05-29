namespace Library.Models.OpenBadges
{
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Metadata about images that represent assertions, achieve or profiles. These properties can typically be represented as just the id string of the image, but using a fleshed-out document allows for including captions and other applicable metadata.
    /// </summary>
    public partial class Image
    {

        /// <summary>
        /// The caption for the image.
        /// </summary>
        [Display(Description = "The caption for the image.")]
        [JsonPropertyName("caption")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Caption { get; set; }

        /// <summary>
        /// The URI or Data URI of the image.
        /// </summary>
        [Display(Description = "The URI or Data URI of the image.")]
        [JsonPropertyName("id")]
        [Required]
        public required string Id { get; set; }

        /// <summary>
        /// MUST be the IRI 'Image'.
        /// </summary>
        [Display(Description = "MUST be the IRI 'Image'.")]
        [JsonPropertyName("type")]
        [Required]
        public required string Type { get; set; }

    }

}