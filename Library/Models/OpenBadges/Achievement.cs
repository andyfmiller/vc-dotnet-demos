namespace Library.Models.OpenBadges
{
    using Library.Models.Converters;
    using Library.Models.OpenBadges.Converters;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json.Serialization;

    /// <summary>
    /// A collection of information about the accomplishment recognized by the Assertion. Many assertions may be created corresponding to one Achievement.
    /// </summary>
    [JsonConverter(typeof(AchievementConverter))]
    public partial class Achievement
    {
        /// <summary>
        /// The type of achievement. This is an extensible vocabulary.
        /// </summary>
        [Display(Description = "The type of achievement. This is an extensible vocabulary.")]
        [JsonPropertyName("achievementType")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public AchievementType? AchievementType { get; set; }

        private ICollection<Alignment> _alignment = new Collection<Alignment>();
        /// <summary>
        /// An object describing which objectives or educational standards this achievement aligns to, if any.
        /// </summary>
        [Display(Description = "An object describing which objectives or educational standards this achievement aligns to, if any.")]
        [JsonPropertyName("alignment")]
        [JsonConverter(typeof(AlignmentConverter))]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ICollection<Alignment>? Alignment
        {
            get => _alignment;
            set => _alignment = value ?? new Collection<Alignment>();
        }

        /// <summary>
        /// Criteria describing how to earn the achievement.
        /// </summary>
        [Display(Description = "Criteria describing how to earn the achievement.")]
        [JsonPropertyName("criteria")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [Required]
        public Criteria? Criteria { get; set; }

        /// <summary>
        /// Credit hours associated with this entity, or credit hours possible. For example 3.0.
        /// </summary>
        [Display(Description = "Credit hours associated with this entity, or credit hours possible. For example 3.0.")]
        [JsonPropertyName("creditsAvailable")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? CreditsAvailable { get; set; }

        /// <summary>
        /// The creator of the achievement.
        /// </summary>
        [Display(Description = "The creator of the achievement.")]
        [JsonPropertyName("creator")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Profile? Creator { get; set; }

        /// <summary>
        /// A short description of the achievement.
        /// </summary>
        [Display(Description = "A short description of the achievement.")]
        [JsonPropertyName("description")]
        [Required]
        public required string Description { get; set; }

        private ICollection<EndorsementCredential> _endorsement = new Collection<EndorsementCredential>();
        /// <summary>
        /// Allows endorsers to make specific claims about the Achievement. These endorsements are signed with a Data Integrity proof format.
        /// </summary>
        [Display(Description = "Allows endorsers to make specific claims about the Achievement. These endorsements are signed with a Data Integrity proof format.")]
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
        /// Allows endorsers to make specific claims about the Achievement. These endorsements are signed with the VC-JWT proof format.
        /// </summary>
        [Display(Description = "Allows endorsers to make specific claims about the Achievement. These endorsements are signed with the VC-JWT proof format.")]
        [JsonPropertyName("endorsementJwt")]
        [JsonConverter(typeof(StringCollectionConverter))]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ICollection<string> EndorsementJwt
        {
            get => _endorsementJwt;
            set => _endorsementJwt = value ?? new Collection<string>();
        }

        /// <summary>
        /// Category, subject, area of study, discipline, or general branch of knowledge. Examples include Business, Education, Psychology, and Technology.
        /// </summary>
        [Display(Description = "Category, subject, area of study, discipline, or general branch of knowledge. Examples include Business, Education, Psychology, and Technology.")]
        [JsonPropertyName("fieldOfStudy")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? FieldOfStudy { get; set; }

        /// <summary>
        /// The code, generally human readable, associated with an achievement.
        /// </summary>
        [Display(Description = "The code, generally human readable, associated with an achievement.")]
        [JsonPropertyName("humanCode")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? HumanCode { get; set; }

        /// <summary>
        /// Unique URI for the Achievement.
        /// </summary>
        [Display(Description = "Unique URI for the Achievement.")]
        [JsonPropertyName("id")]
        [Required]
        public required string Id { get; set; }

        /// <summary>
        /// An image representing the achievement.
        /// </summary>
        [Display(Description = "An image representing the achievement.")]
        [JsonPropertyName("image")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Image? Image { get; set; }

        /// <summary>
        /// The language of the achievement.
        /// </summary>
        [Display(Description = "The language of the achievement.")]
        [JsonPropertyName("inLanguage")]
        [RegularExpression(@"^[a-z]{2,4}(-[A-Z][a-z]{3})?(-([A-Z]{2}|[0-9]{3}))?$")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? InLanguage { get; set; }

        /// <summary>
        /// The name of the achievement.
        /// </summary>
        [Display(Description = "The name of the achievement.")]
        [JsonPropertyName("name")]
        [Required]
        public required string Name { get; set; }

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

        private ICollection<Related> _related = new Collection<Related>();
        /// <summary>
        /// The related property identifies another Achievement that should be considered the same for most purposes. It is primarily intended to identify alternate language editions or previous versions of Achievements.
        /// </summary>
        [Display(Description = "The related property identifies another Achievement that should be considered the same for most purposes. It is primarily intended to identify alternate language editions or previous versions of Achievements.")]
        [JsonPropertyName("related")]
        [JsonConverter(typeof(RelatedConverter))]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ICollection<Related>? Related
        {
            get => _related;
            set => _related = value ?? new Collection<Related>();
        }

        private ICollection<ResultDescription> _resultDescription = new Collection<ResultDescription>();
        /// <summary>
        /// The set of result descriptions that may be asserted as results with this achievement.
        /// </summary>
        [Display(Description = "The set of result descriptions that may be asserted as results with this achievement.")]
        [JsonPropertyName("resultDescription")]
        [JsonConverter(typeof(ResultDescriptionConverter))]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ICollection<ResultDescription>? ResultDescription
        {
            get => _resultDescription;
            set => _resultDescription = value ?? new Collection<ResultDescription>();
        }

        /// <summary>
        /// Name given to the focus, concentration, or specific area of study defined in the achievement. Examples include 'Entrepreneurship', 'Technical Communication', and 'Finance'.
        /// </summary>
        [Display(Description = "Name given to the focus, concentration, or specific area of study defined in the achievement. Examples include 'Entrepreneurship', 'Technical Communication', and 'Finance'.")]
        [JsonPropertyName("specialization")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Specialization { get; set; }

        private ICollection<string> _tag = new Collection<string>();
        /// <summary>
        /// One or more short, human-friendly, searchable, keywords that describe the type of achievement.
        /// </summary>
        [Display(Description = "One or more short, human-friendly, searchable, keywords that describe the type of achievement.")]
        [JsonPropertyName("tag")]
        [JsonConverter(typeof(StringCollectionConverter))]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ICollection<string>? Tag
        {
            get => _tag;
            set => _tag = value ?? new Collection<string>();
        }

        /// <summary>
        /// The type MUST include the IRI 'Achievement'.
        /// </summary>
        [Display(Description = "The type MUST include the IRI 'Achievement'.")]
        [JsonPropertyName("type")]
        [Required]
        [MinLength(1)]
        [JsonConverter(typeof(StringCollectionConverter))]
        public ICollection<string> Type { get; set; } = new Collection<string>();

        /// <summary>
        /// The version property allows issuers to set a version string for an Achievement. This is particularly useful when replacing a previous version with an update.
        /// </summary>
        [Display(Description = "The version property allows issuers to set a version string for an Achievement. This is particularly useful when replacing a previous version with an update.")]
        [JsonPropertyName("version")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Version { get; set; }

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