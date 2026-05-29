namespace Library.Models.Vc
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Terms of use can be utilized by an issuer or a holder to communicate the terms under which a verifiable credential or verifiable presentation was issued
    /// </summary>
    public partial class TermsOfUse
    {

        /// <summary>
        /// The value MUST be a URI identifying the term of use.
        /// </summary>
        [Display(Description = "The value MUST be a URI identifying the term of use.")]
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        /// <summary>
        /// The value MUST identify the type of the terms of use.
        /// </summary>
        [Display(Description = "The value MUST identify the type of the terms of use.")]
        [JsonPropertyName("type")]
        [Required]
        public required string Type { get; set; }

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