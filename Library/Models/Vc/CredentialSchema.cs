namespace Library.Models.Vc
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Identify the type and location of a data schema.
    /// </summary>
    public partial class CredentialSchema
    {

        /// <summary>
        /// The value MUST be a URI identifying the schema file. One instance of `CredentialSchema` MUST have an `id` that is the URL of the JSON Schema for this credential defined by this specification.
        /// </summary>
        [Display(Description = "The value MUST be a URI identifying the schema file. One instance of `CredentialSchema` MUST have an `id` that is the URL of the JSON Schema for this credential defined by this specification.")]
        [JsonPropertyName("id")]
        [Required]
        public required string Id { get; set; }

        /// <summary>
        /// The value MUST identify the type of data schema validation. One instance of `CredentialSchema` MUST have a `type` of '1EdTechJsonSchemaValidator2019'.
        /// </summary>
        [Display(Description = "The value MUST identify the type of data schema validation. One instance of `CredentialSchema` MUST have a `type` of '1EdTechJsonSchemaValidator2019'.")]
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