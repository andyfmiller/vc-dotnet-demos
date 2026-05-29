namespace Library.Models.OpenBadges
{
    using Library.Models.Converters;
    using Library.Models.OpenBadges.Converters;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Describes an alignment between an achievement and a node in an educational framework.
    /// </summary>
    public partial class Alignment
    {
        /// <summary>
        /// If applicable, a locally unique string identifier that identifies the alignment target within its framework and/or targetUrl.
        /// </summary>
        [Display(Description = "If applicable, a locally unique string identifier that identifies the alignment target within its framework and/or targetUrl.")]
        [JsonPropertyName("targetCode")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? TargetCode { get; set; }

        /// <summary>
        /// Short description of the alignment target.
        /// </summary>
        [Display(Description = "Short description of the alignment target.")]
        [JsonPropertyName("targetDescription")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? TargetDescription { get; set; }

        /// <summary>
        /// Name of the framework the alignment target.
        /// </summary>
        [Display(Description = "Name of the framework the alignment target.")]
        [JsonPropertyName("targetFramework")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? TargetFramework { get; set; }

        /// <summary>
        /// Name of the alignment.
        /// </summary>
        [Display(Description = "Name of the alignment.")]
        [JsonPropertyName("targetName")]
        [Required]
        public required string TargetName { get; set; }

        /// <summary>
        /// The type of the alignment target node.
        /// </summary>
        [Display(Description = "The type of the alignment target node.")]
        [JsonPropertyName("targetType")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public AlignmentTargetType? TargetType { get; set; }

        /// <summary>
        /// URL linking to the official description of the alignment target, for example an individual standard within an educational framework.
        /// </summary>
        [Display(Description = "URL linking to the official description of the alignment target, for example an individual standard within an educational framework.")]
        [JsonPropertyName("targetUrl")]
        [Required]
        public required string TargetUrl { get; set; }

        /// <summary>
        /// The value of the type property MUST be an unordered set. One of the items MUST be the IRI 'Alignment'.
        /// </summary>
        [Display(Description = "The value of the type property MUST be an unordered set. One of the items MUST be the IRI 'Alignment'.")]
        [JsonPropertyName("type")]
        [Required]
        [MinLength(1)]
        [JsonConverter(typeof(StringCollectionConverter))]
        public ICollection<string> Type { get; set; } = new Collection<string>();

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