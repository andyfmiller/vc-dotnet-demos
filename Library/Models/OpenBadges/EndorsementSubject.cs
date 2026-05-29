namespace Library.Models.OpenBadges
{
    using Library.Models.Converters;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json.Serialization;

    /// <summary>
    /// A collection of information about the subject of the endorsement.
    /// </summary>
    public partial class EndorsementSubject
    {
        /// <summary>
        /// Allows endorsers to make a simple claim in writing about the entity.
        /// </summary>
        [Display(Description = "Allows endorsers to make a simple claim in writing about the entity.")]
        [JsonPropertyName("endorsementComment")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? EndorsementComment { get; set; }

        /// <summary>
        /// The identifier of the individual, entity, organization, assertion, or achievement that is endorsed.
        /// </summary>
        [Display(Description = "The identifier of the individual, entity, organization, assertion, or achievement that is endorsed.")]
        [JsonPropertyName("id")]
        [Required]
        public required string Id { get; set; }

        /// <summary>
        /// The value of the type property MUST be an unordered set. One of the items MUST be the URI 'EndorsementSubject'.
        /// </summary>
        [Display(Description = "The value of the type property MUST be an unordered set. One of the items MUST be the URI 'EndorsementSubject'.")]
        [JsonPropertyName("type")]
        [Required]
        [MinLength(1)]
        [JsonConverter(typeof(StringCollectionConverter))]
        public ICollection<string> Type { get; set; } = new Collection<string>();

        private IDictionary<string, object>? _additionalProperties;

        /// <summary>
        /// Additional properties not defined in the schema.
        /// </summary>
        [JsonExtensionData]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, object> AdditionalProperties
        {
            get { return _additionalProperties ?? (_additionalProperties = new Dictionary<string, object>()); }
            set { _additionalProperties = value; }
        }
    }
}