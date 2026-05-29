namespace Library.Models.Vc
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using System.ComponentModel.DataAnnotations;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Claims about the credential subject. Maps to Credential Subject as defined in the [[VC-DATA-MODEL-2.0]].
    /// </summary>
    public partial class CredentialSubject
    {
        /// <summary>
        /// The identity of the credential subject.
        /// </summary>
        [Display(Description = "The identity of the credential subject.")]
        [JsonPropertyName("id")]
        public virtual string? Id { get; set; }

        private ICollection<string> _type = new Collection<string>();

        /// <summary>
        /// The value of the type property MUST be an unordered set. One of the items MUST be the URI 'CredentialSubject'.
        /// </summary>
        [Display(Description = "The value of the type property MUST be an unordered set. One of the items MUST be the URI 'CredentialSubject'.")]
        [JsonPropertyName("type")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [Required]
        [MinLength(1)]
        public virtual ICollection<string>? Type
        {
            get => _type.Count == 0 ? null : _type;
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