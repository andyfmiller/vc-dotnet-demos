namespace Library.Models.OpenBadges
{
    using Library.Models.Converters;
    using Library.Models.OpenBadges.Converters;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Describes a possible achievement result.
    /// </summary>
    public partial class ResultDescription
    {
        private ICollection<Alignment> _alignment = new Collection<Alignment>();
        /// <summary>
        /// Alignments between this result description and nodes in external frameworks.
        /// </summary>
        [Display(Description = "Alignments between this result description and nodes in external frameworks.")]
        [JsonPropertyName("alignment")]
        [JsonConverter(typeof(AlignmentConverter))]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ICollection<Alignment>? Alignment
        {
            get => _alignment.Count == 0 ? null : _alignment;
            set => _alignment = value ?? new Collection<Alignment>();
        }

        private ICollection<string> _allowedValue = new Collection<string>();
        /// <summary>
        /// An ordered list of allowed values. The values should be ordered from low to high as determined by the achievement creator.
        /// </summary>
        [Display(Description = "An ordered list of allowed values. The values should be ordered from low to high as determined by the achievement creator.")]
        [JsonPropertyName("allowedValue")]
        [JsonConverter(typeof(StringCollectionConverter))]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ICollection<string>? AllowedValue
        {
            get => _allowedValue.Count == 0 ? null : _allowedValue;
            set => _allowedValue = value ?? new Collection<string>();
        }

        /// <summary>
        /// The unique URI for this result description. Required so a result can link to this result description.
        /// </summary>
        [Display(Description = "The unique URI for this result description. Required so a result can link to this result description.")]
        [JsonPropertyName("id")]
        [Required]
        public required string Id { get; set; }

        /// <summary>
        /// The name of the result.
        /// </summary>
        [Display(Description = "The name of the result.")]
        [JsonPropertyName("name")]
        [Required]
        public required string Name { get; set; }

        /// <summary>
        /// The `id` of the rubric criterion level required to pass as determined by the achievement creator.
        /// </summary>
        [Display(Description = "The `id` of the rubric criterion level required to pass as determined by the achievement creator.")]
        [JsonPropertyName("requiredLevel")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? RequiredLevel { get; set; }

        /// <summary>
        /// A value from `allowedValue` or within the range of `valueMin` to `valueMax` required to pass as determined by the achievement creator.
        /// </summary>
        [Display(Description = "A value from `allowedValue` or within the range of `valueMin` to `valueMax` required to pass as determined by the achievement creator.")]
        [JsonPropertyName("requiredValue")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? RequiredValue { get; set; }

        /// <summary>
        /// The type of result this description represents. This is an extensible enumerated vocabulary.
        /// </summary>
        [Display(Description = "The type of result this description represents. This is an extensible enumerated vocabulary.")]
        [JsonPropertyName("resultType")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ResultType? ResultType { get; set; }

        private ICollection<RubricCriterionLevel> _rubricCriterionLevel = new Collection<RubricCriterionLevel>();
        /// <summary>
        /// An ordered array of rubric criterion levels that may be asserted in the linked result. The levels should be ordered from low to high as determined by the achievement creator.
        /// </summary>
        [Display(Description = "An ordered array of rubric criterion levels that may be asserted in the linked result. The levels should be ordered from low to high as determined by the achievement creator.")]
        [JsonPropertyName("rubricCriterionLevel")]
        [JsonConverter(typeof(RubricCriterionLevelConverter))]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ICollection<RubricCriterionLevel>? RubricCriterionLevel
        {
            get => _rubricCriterionLevel.Count == 0 ? null : _rubricCriterionLevel;
            set => _rubricCriterionLevel = value ?? new Collection<RubricCriterionLevel>();
        }

        /// <summary>
        /// The value of the type property MUST be an unordered set. One of the items MUST be the IRI 'ResultDescription'.
        /// </summary>
        [Display(Description = "The value of the type property MUST be an unordered set. One of the items MUST be the IRI 'ResultDescription'.")]
        [JsonPropertyName("type")]
        [Required]
        [MinLength(1)]
        [JsonConverter(typeof(StringCollectionConverter))]
        public ICollection<string> Type { get; set; } = new Collection<string>();

        /// <summary>
        /// The maximum possible `value` that may be asserted in a linked result.
        /// </summary>
        [Display(Description = "The maximum possible `value` that may be asserted in a linked result.")]
        [JsonPropertyName("valueMax")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ValueMax { get; set; }

        /// <summary>
        /// The minimum possible `value` that may be asserted in a linked result.
        /// </summary>
        [Display(Description = "The minimum possible `value` that may be asserted in a linked result.")]
        [JsonPropertyName("valueMin")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ValueMin { get; set; }

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