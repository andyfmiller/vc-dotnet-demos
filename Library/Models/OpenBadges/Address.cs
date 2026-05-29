namespace Library.Models.OpenBadges
{
    using Library.Models.Converters;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json.Serialization;

    /// <summary>
    /// An address for the described entity.
    /// </summary>
    public partial class Address
    {
        /// <summary>
        /// A country.
        /// </summary>
        [Display(Description = "A country name.")]
        [JsonPropertyName("addressCountry")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? AddressCountry { get; set; }

        /// <summary>
        /// A country code. The value must be a ISO 3166-1 alpha-2 country code [[ISO3166-1]].
        /// </summary>
        [Display(Description = "A country code. The value must be a ISO 3166-1 alpha-2 country code [[ISO3166-1]].")]
        [JsonPropertyName("addressCountryCode")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? AddressCountryCode { get; set; }

        /// <summary>
        /// A locality within the region.
        /// </summary>
        [Display(Description = "A locality within the region.")]
        [JsonPropertyName("addressLocality")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? AddressLocality { get; set; }

        /// <summary>
        /// A region within the country.
        /// </summary>
        [Display(Description = "A region within the country.")]
        [JsonPropertyName("addressRegion")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? AddressRegion { get; set; }

        /// <summary>
        /// Geographic coordinates for the address.
        /// </summary>
        [Display(Description = "Geographic coordinates for the address.")]
        [JsonPropertyName("geo")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public GeoCoordinates? Geo { get; set; }

        /// <summary>
        /// A postal code.
        /// </summary>
        [Display(Description = "A postal code.")]
        [JsonPropertyName("postalCode")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? PostalCode { get; set; }

        /// <summary>
        /// A post office box number for PO box addresses.
        /// </summary>
        [Display(Description = "A post office box number for PO box addresses.")]
        [JsonPropertyName("postOfficeBoxNumber")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? PostOfficeBoxNumber { get; set; }

        /// <summary>
        /// A street address within the locality.
        /// </summary>
        [Display(Description = "A street address within the locality.")]
        [JsonPropertyName("streetAddress")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? StreetAddress { get; set; }

        /// <summary>
        /// The value of the type property MUST be an unordered set. One of the items MUST be the IRI 'Address'.
        /// </summary>
        [Display(Description = "The value of the type property MUST be an unordered set. One of the items MUST be the IRI 'Address'.")]
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