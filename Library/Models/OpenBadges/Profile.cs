namespace Library.Models.OpenBadges
{
    using Library.Models.Converters;
    using Library.Models.OpenBadges.Converters;
    using Library.Models.Vc;
    using Library.Models.Vc.Converters;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json.Serialization;

    /// <summary>
    /// A Profile is a collection of information that describes the entity or organization using Open Badges. Issuers must be represented as Profiles, and endorsers, or other entities may also be represented using this vocabulary. Each Profile that represents an Issuer may be referenced in many BadgeClasses that it has defined. Anyone can create and host an Issuer file to start issuing Open Badges. Issuers may also serve as recipients of Open Badges, often identified within an Assertion by specific properties, like their url or contact email address.
    /// </summary>
    [JsonConverter(typeof(ProfileConverter))]
    public partial class Profile : Issuer
    {
        /// <summary>
        /// Additional name. Includes what is often referred to as 'middle name' in the western world.
        /// </summary>
        [Display(Description = "Additional name. Includes what is often referred to as 'middle name' in the western world.")]
        [JsonPropertyName("additionalName")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? AdditionalName { get; set; }

        /// <summary>
        /// Physical address of the entity or organization.
        /// </summary>
        [Display(Description = "Physical address of the entity or organization.")]
        [JsonPropertyName("address")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Address? Address { get; set; }

        /// <summary>
        /// Birthdate of the person.
        /// </summary>
        [Display(Description = "Birthdate of the person.")]
        [JsonPropertyName("dateOfBirth")]
        [JsonConverter(typeof(DateFormatConverter))]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTime? DateOfBirth { get; set; }

        /// <summary>
        /// A short description of the issuer entity or organization.
        /// </summary>
        [Display(Description = "A short description of the issuer entity or organization.")]
        [JsonPropertyName("description")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description { get; set; }

        /// <summary>
        /// An email address.
        /// </summary>
        [Display(Description = "An email address.")]
        [JsonPropertyName("email")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Email { get; set; }

        private ICollection<EndorsementCredential> _endorsement = new Collection<EndorsementCredential>();
        /// <summary>
        /// Allows endorsers to make specific claims about the individual or organization represented by this profile. These endorsements are signed with a Data Integrity proof format.
        /// </summary>
        [Display(Description = "Allows endorsers to make specific claims about the individual or organization represented by this profile. These endorsements are signed with a Data Integrity proof format.")]
        [JsonPropertyName("endorsement")]
        [JsonConverter(typeof(EndorsementConverter))]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ICollection<EndorsementCredential> Endorsement
        {
            get => _endorsement;
            set => _endorsement = value ?? new Collection<EndorsementCredential>();
        }

        private ICollection<string> _endorsementJwt = new Collection<string>();
        /// <summary>
        /// Allows endorsers to make specific claims about the individual or organization represented by this profile. These endorsements are signed with the VC-JWT proof format.
        /// </summary>
        [Display(Description = "Allows endorsers to make specific claims about the individual or organization represented by this profile. These endorsements are signed with the VC-JWT proof format.")]
        [JsonPropertyName("endorsementJwt")]
        [JsonConverter(typeof(StringCollectionConverter))]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ICollection<string> EndorsementJwt
        {
            get => _endorsementJwt;
            set => _endorsementJwt = value ?? new Collection<string>();
        }

        /// <summary>
        /// Family name. In the western world, often referred to as the 'last name' of a person.
        /// </summary>
        [Display(Description = "Family name. In the western world, often referred to as the 'last name' of a person.")]
        [JsonPropertyName("familyName")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? FamilyName { get; set; }

        /// <summary>
        /// Family name prefix. As used in some locales, this is the leading part of a family name (e.g. 'de' in the name 'de Boer').
        /// </summary>
        [Display(Description = "Family name prefix. As used in some locales, this is the leading part of a family name (e.g. 'de' in the name 'de Boer').")]
        [JsonPropertyName("familyNamePrefix")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? FamilyNamePrefix { get; set; }

        /// <summary>
        /// Given name. In the western world, often referred to as the 'first name' of a person.
        /// </summary>
        [Display(Description = "Given name. In the western world, often referred to as the 'first name' of a person.")]
        [JsonPropertyName("givenName")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? GivenName { get; set; }

        /// <summary>
        /// Honorific prefix(es) preceding a person's name (e.g. 'Dr', 'Mrs' or 'Mr').
        /// </summary>
        [Display(Description = "Honorific prefix(es) preceding a person's name (e.g. 'Dr', 'Mrs' or 'Mr').")]
        [JsonPropertyName("honorificPrefix")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? HonorificPrefix { get; set; }

        /// <summary>
        /// Honorific suffix(es) following a person's name (e.g. 'M.D, PhD').
        /// </summary>
        [Display(Description = "Honorific suffix(es) following a person's name (e.g. 'M.D, PhD').")]
        [JsonPropertyName("honorificSuffix")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? HonorificSuffix { get; set; }

        /// <summary>
        /// An image representing the entity.
        /// </summary>
        [Display(Description = "An image representing the entity.")]
        [JsonPropertyName("image")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Image? Image { get; set; }

        /// <summary>
        /// The name of the entity or organization.
        /// </summary>
        [Display(Description = "The name of the entity or organization.")]
        [JsonPropertyName("name")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Name { get; set; }

        /// <summary>
        /// If the entity is an organization, `official` is the name of an authorized official of the organization.
        /// </summary>
        [Display(Description = "If the entity is an organization, `official` is the name of an authorized official of the organization.")]
        [JsonPropertyName("official")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Official { get; set; }

        private ICollection<IdentifierEntry> _otherIdentifier = new Collection<IdentifierEntry>();
        /// <summary>
        /// A list of identifiers for the described entity.
        /// </summary>
        [Display(Description = "A list of identifiers for the described entity.")]
        [JsonPropertyName("otherIdentifier")]
        [JsonConverter(typeof(IdentifierEntryConverter))]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ICollection<IdentifierEntry>? OtherIdentifier
        {
            get => _otherIdentifier;
            set => _otherIdentifier = value ?? new Collection<IdentifierEntry>();
        }

        /// <summary>
        /// The parent organization of this entity.
        /// </summary>
        [Display(Description = "The parent organization of this entity.")]
        [JsonPropertyName("parentOrg")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Profile? ParentOrg { get; set; }

        /// <summary>
        /// Patronymic name.
        /// </summary>
        [Display(Description = "Patronymic name.")]
        [JsonPropertyName("patronymicName")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? PatronymicName { get; set; }

        /// <summary>
        /// A phone number.
        /// </summary>
        [Display(Description = "A phone number.")]
        [JsonPropertyName("phone")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Phone { get; set; }

        /// <summary>
        /// The value of the type property MUST be an unordered set. One of the items MUST be the IRI 'Profile'.
        /// </summary>
        [Display(Description = "The value of the type property MUST be an unordered set. One of the items MUST be the IRI 'Profile'.")]
        [JsonPropertyName("type")]
        [Required]
        [MinLength(1)]
        [JsonConverter(typeof(TypeConverter))]
        public ICollection<string> Type { get; set; } = new Collection<string>();

        /// <summary>
        /// The homepage or social media profile of the entity, whether individual or institutional. Should be a URL/URI Accessible via HTTP.
        /// </summary>
        [Display(Description = "The homepage or social media profile of the entity, whether individual or institutional. Should be a URL/URI Accessible via HTTP.")]
        [JsonPropertyName("url")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Url { get; set; }
    }
}