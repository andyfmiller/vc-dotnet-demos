namespace Library.Models.OpenBadges
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json.Serialization;

    /// <summary>
    /// The geographic coordinates of a location.
    /// </summary>
    public partial class GeoCoordinates
    {
        /// <summary>
        /// The latitude of the location [[WGS84]].
        /// </summary>
        [Display(Description = "The latitude of the location [[WGS84]].")]
        [JsonPropertyName("latitude")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? Latitude { get; set; }

        /// <summary>
        /// The longitude of the location [[WGS84]].
        /// </summary>
        [Display(Description = "The longitude of the location [[WGS84]].")]
        [JsonPropertyName("longitude")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? Longitude { get; set; }

        /// <summary>
        /// MUST be the IRI 'GeoCoordinates'.
        /// </summary>
        [Display(Description = "MUST be the IRI 'GeoCoordinates'.")]
        [JsonPropertyName("type")]
        [Required]
        public required string Type { get; set; }

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