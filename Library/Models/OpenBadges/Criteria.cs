namespace Library.Models.OpenBadges
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Descriptive metadata about the achievements necessary to be recognized with an assertion of a particular achievement. This data is added to the Achievement class so that it may be rendered when the achievement assertion is displayed, instead of simply a link to human-readable criteria external to the achievement. Embedding criteria allows either enhancement of an external criteria page or increased portability and ease of use by allowing issuers to skip hosting the formerly-required external criteria page altogether. Criteria is used to allow would-be recipients to learn what is required of them to be recognized with an assertion of a particular achievement. It is also used after the assertion is awarded to a recipient to let those inspecting earned achievements know the general requirements that the recipients met in order to earn it.
    /// </summary>
    public partial class Criteria
    {
        /// <summary>
        /// The URI of a webpage that describes in a human-readable format the criteria for the achievement.
        /// </summary>
        [Display(Description = "The URI of a webpage that describes in a human-readable format the criteria for the achievement.")]
        [JsonPropertyName("id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Id { get; set; }

        /// <summary>
        /// A narrative of what is needed to earn the achievement. Markdown is allowed.
        /// </summary>
        [Display(Description = "A narrative of what is needed to earn the achievement. Markdown is allowed.")]
        [JsonPropertyName("narrative")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Narrative { get; set; }

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