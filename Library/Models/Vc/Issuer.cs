namespace Library.Models.Vc
{
    using Library.Models.Vc.Converters;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Represents the issuer of a verifiable credential. The issuer can be expressed as either a URL 
    /// or an object containing an id property whose value is a URL. The issuer selects this URL to 
    /// identify itself in a globally unambiguous way. It is RECOMMENDED that the URL be one which, 
    /// if dereferenced, results in a controlled identifier document about the issuer.
    /// </summary>
    [JsonConverter(typeof(IssuerConverter))]
    public partial class Issuer : IValidatableObject
    {
        /// <summary>
        /// The URL that identifies the issuer in a globally unambiguous way.
        /// </summary>
        [Display(Description = "The URL that identifies the issuer in a globally unambiguous way.")]
        [Required]
        [JsonPropertyName("id")]
        public required string Id { get; set; }

        private Dictionary<string, object>? _additionalProperties;

        /// <summary>
        /// Additional properties about the issuer.
        /// </summary>
        [JsonExtensionData]
        [JsonPropertyName("additionalProperties")]
        [Display(Name = "Additional Properties", Description = "Additional properties about the issuer.")]
        public Dictionary<string, object> AdditionalProperties
        {
            get { return _additionalProperties ??= []; }
            set { _additionalProperties = value; }
        }

        /// <summary>
        /// Creates an Issuer from a URL string.
        /// </summary>
        /// <param name="url">The URL that identifies the issuer.</param>
        /// <returns>An Issuer instance.</returns>
        public static Issuer FromUrl(string url) => new() { Id = url };

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Id must always be present and not empty
            if (string.IsNullOrWhiteSpace(Id))
            {
                yield return new ValidationResult(
                    "The id property is required and must be a URL.",
                    new[] { nameof(Id) });
            }
        }
    }
}