namespace Library.Models.Vc
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json.Serialization;

    /// <summary>
    /// The information in RefreshService is used to refresh the verifiable credential.
    /// </summary>
    public partial class RefreshService
    {

        /// <summary>
        /// The value MUST be the URL of the issuer's refresh service.
        /// </summary>
        [Display(Description = "The value MUST be the URL of the issuer's refresh service.")]
        [JsonPropertyName("id")]
        [Required]
        public required string Id { get; set; }

        /// <summary>
        /// The name of the refresh service method.
        /// </summary>
        [Display(Description = "The name of the refresh service method.")]
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