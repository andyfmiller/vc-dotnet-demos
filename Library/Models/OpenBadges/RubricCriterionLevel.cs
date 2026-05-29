namespace Library.Models.OpenBadges
{
    using Library.Models.Converters;
    using Library.Models.OpenBadges.Converters;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Describes a rubric criterion level.
    /// </summary>
    public partial class RubricCriterionLevel
    {
        private ICollection<Alignment> _alignment = new Collection<Alignment>();
        /// <summary>
        /// Alignments between this rubric criterion level and a rubric criterion levels defined in external frameworks.
        /// </summary>
        [Display(Description = "Alignments between this rubric criterion level and a rubric criterion levels defined in external frameworks.")]
        [JsonPropertyName("alignment")]
        [JsonConverter(typeof(AlignmentConverter))]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ICollection<Alignment>? Alignment
        {
            get => _alignment.Count == 0 ? null : _alignment;
            set => _alignment = value ?? new Collection<Alignment>();
        }

        /// <summary>
        /// Description of the rubric criterion level.
        /// </summary>
        [Display(Description = "Description of the rubric criterion level.")]
        [JsonPropertyName("description")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description { get; set; }

        /// <summary>
        /// The unique URI for this rubric criterion level. Required so a result can link to this rubric criterion level.
        /// </summary>
        [Display(Description = "The unique URI for this rubric criterion level. Required so a result can link to this rubric criterion level.")]
        [JsonPropertyName("id")]
        [Required]
        public required string Id { get; set; }

        /// <summary>
        /// The rubric performance level in terms of success.
        /// </summary>
        [Display(Description = "The rubric performance level in terms of success.")]
        [JsonPropertyName("level")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Level { get; set; }

        /// <summary>
        /// The name of the rubric criterion level.
        /// </summary>
        [Display(Description = "The name of the rubric criterion level.")]
        [JsonPropertyName("name")]
        [Required]
        public required string Name { get; set; }

        /// <summary>
        /// The points associated with this rubric criterion level.
        /// </summary>
        [Display(Description = "The points associated with this rubric criterion level.")]
        [JsonPropertyName("points")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Points { get; set; }

        /// <summary>
        /// The value of the type property MUST be an unordered set. One of the items MUST be the IRI 'RubricCriterionLevel'.
        /// </summary>
        [Display(Description = "The value of the type property MUST be an unordered set. One of the items MUST be the IRI 'RubricCriterionLevel'.")]
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