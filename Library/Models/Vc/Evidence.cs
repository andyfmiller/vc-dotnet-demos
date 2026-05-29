namespace Library.Models.Vc
{
    using Library.Models.Converters;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Descriptive metadata about evidence related to the achievement assertion. Each instance of the evidence class present in an assertion corresponds to one entity, though a single entry can describe a set of items collectively. There may be multiple evidence entries referenced from an assertion. The narrative property is also in scope of the assertion class to provide an overall description of the achievement related to the assertion in rich text. It is used here to provide a narrative of achievement of the specific entity described. If both the description and narrative properties are present, displayers can assume the narrative value goes into more detail and is not simply a recapitulation of description.
    /// </summary>
    public partial class Evidence
    {
        /// <summary>
        /// A description of the intended audience for a piece of evidence.
        /// </summary>
        [Display(Description = "A description of the intended audience for a piece of evidence.")]
        [JsonPropertyName("audience")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Audience { get; set; }

        /// <summary>
        /// A longer description of the evidence.
        /// </summary>
        [Display(Description = "A longer description of the evidence.")]
        [JsonPropertyName("description")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description { get; set; }

        /// <summary>
        /// A string that describes the type of evidence. For example, Poetry, Prose, Film.
        /// </summary>
        [Display(Description = "A string that describes the type of evidence. For example, Poetry, Prose, Film.")]
        [JsonPropertyName("genre")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Genre { get; set; }

        /// <summary>
        /// The URL of a webpage presenting evidence of achievement or the evidence encoded as a Data URI. The schema of the webpage is undefined.
        /// </summary>
        [Display(Description = "The URL of a webpage presenting evidence of achievement or the evidence encoded as a Data URI. The schema of the webpage is undefined.")]
        [JsonPropertyName("id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Id { get; set; }

        /// <summary>
        /// A descriptive title of the evidence.
        /// </summary>
        [Display(Description = "A descriptive title of the evidence.")]
        [JsonPropertyName("name")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Name { get; set; }

        /// <summary>
        /// A narrative that describes the evidence and process of achievement that led to an assertion.
        /// </summary>
        [Display(Description = "A narrative that describes the evidence and process of achievement that led to an assertion.")]
        [JsonPropertyName("narrative")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Narrative { get; set; }

        private ICollection<string> _type = new Collection<string>();
        /// <summary>
        /// The value of the type property MUST be an unordered set. One of the items MUST be the IRI 'Evidence'.
        /// </summary>
        [Display(Description = "The value of the type property MUST be an unordered set. One of the items MUST be the IRI 'Evidence'.")]
        [JsonPropertyName("type")]
        [Required]
        [MinLength(1)]
        [JsonConverter(typeof(StringCollectionConverter))]
        public ICollection<string> Type
        {
            get => _type;
            set => _type = value ?? new Collection<string>();
        }

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