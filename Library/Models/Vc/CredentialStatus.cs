namespace Library.Models.Vc
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json.Serialization;

    /// <summary>
    /// The information in CredentialStatus is used to discover information about the current status of a verifiable credential, such as whether it is suspended or revoked.
    /// </summary>
    public partial class CredentialStatus
    {

        /// <summary>
        /// The value MUST be the URL of the issuer's credential status method.
        /// </summary>
        [Display(Description = "The value MUST be the URL of the issuer's credential status method.")]
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        /// <summary>
        /// The name of the credential status method.
        /// </summary>
        [Display(Description = "The name of the credential status method.")]
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