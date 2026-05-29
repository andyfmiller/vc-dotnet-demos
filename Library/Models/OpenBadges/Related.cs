namespace Library.Models.OpenBadges
{
    using Library.Models.Converters;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Identifies a related achievement.
    /// </summary>
    public partial class Related
    {
        /// <summary>
        /// The related achievement.
        /// </summary>
        [Display(Description = "The related achievement.")]
        [JsonPropertyName("id")]
        [Required]
        public required string Id { get; set; }

        /// <summary>
        /// The language of the related achievement.
        /// </summary>
        [Display(Description = "The language of the related achievement.")]
        [JsonPropertyName("inLanguage")]
        [RegularExpression(@"^[a-z]{2,4}(-[A-Z][a-z]{3})?(-([A-Z]{2}|[0-9]{3}))?$")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? InLanguage { get; set; }

        /// <summary>
        /// The value of the type property MUST be an unordered set. One of the items MUST be the IRI 'Related'.
        /// </summary>
        [Display(Description = "The value of the type property MUST be an unordered set. One of the items MUST be the IRI 'Related'.")]
        [JsonPropertyName("type")]
        [Required]
        [MinLength(1)]
        [JsonConverter(typeof(StringCollectionConverter))]
        public ICollection<string> Type { get; set; } = new Collection<string>();

        /// <summary>
        /// The version of the related achievement.
        /// </summary>
        [Display(Description = "The version of the related achievement.")]
        [JsonPropertyName("version")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Version { get; set; }

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