namespace Library.Models.OpenBadges
{
    using Library.Models.Converters;
    using Library.Models.OpenBadges.Converters;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Describes a result that was achieved.
    /// </summary>
    public partial class Result
    {
        /// <summary>
        /// If the result represents an achieved rubric criterion level (e.g. Mastered), the value is the `id` of the RubricCriterionLevel in linked ResultDescription.
        /// </summary>
        [Display(Description = "If the result represents an achieved rubric criterion level (e.g. Mastered), the value is the `id` of the RubricCriterionLevel in linked ResultDescription.")]
        [JsonPropertyName("achievedLevel")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? AchievedLevel { get; set; }

        private ICollection<Alignment> _alignment = new Collection<Alignment>();
        /// <summary>
        /// The alignments between this result and nodes in external frameworks. This set of alignments are in addition to the set of alignments defined in the corresponding ResultDescription object.
        /// </summary>
        [Display(Description = "The alignments between this result and nodes in external frameworks. This set of alignments are in addition to the set of alignments defined in the corresponding ResultDescription object.")]
        [JsonPropertyName("alignment")]
        [JsonConverter(typeof(AlignmentConverter))]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ICollection<Alignment>? Alignment
        {
            get => _alignment.Count == 0 ? null : _alignment;
            set => _alignment = value ?? new Collection<Alignment>();
        }

        /// <summary>
        /// An achievement can have many result descriptions describing possible results. The value of `resultDescription` is the `id` of the result description linked to this result. The linked result description must be in the achievement that is being asserted.
        /// </summary>
        [Display(Description = "An achievement can have many result descriptions describing possible results. The value of `resultDescription` is the `id` of the result description linked to this result. The linked result description must be in the achievement that is being asserted.")]
        [JsonPropertyName("resultDescription")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ResultDescription { get; set; }

        /// <summary>
        /// The status of the achievement. Required if `resultType` of the linked ResultDescription is Status.
        /// </summary>
        [Display(Description = "The status of the achievement. Required if `resultType` of the linked ResultDescription is Status.")]
        [JsonPropertyName("status")]
        [JsonConverter(typeof(JsonStringEnumConverter<ResultStatusType>))]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ResultStatusType? Status { get; set; }

        /// <summary>
        /// The value of the type property MUST be an unordered set. One of the items MUST be the IRI 'Result'.
        /// </summary>
        [Display(Description = "The value of the type property MUST be an unordered set. One of the items MUST be the IRI 'Result'.")]
        [JsonPropertyName("type")]
        [Required]
        [MinLength(1)]
        [JsonConverter(typeof(StringCollectionConverter))]
        public ICollection<string> Type { get; set; } = new Collection<string>();

        /// <summary>
        /// A string representing the result of the performance, or demonstration, of the achievement. For example, 'A' if the recipient received an A grade in class.
        /// </summary>
        [Display(Description = "A string representing the result of the performance, or demonstration, of the achievement. For example, 'A' if the recipient received an A grade in class.")]
        [JsonPropertyName("value")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Value { get; set; }

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