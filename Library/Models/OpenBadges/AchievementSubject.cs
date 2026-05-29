using Library.Models.Converters;
using Library.Models.OpenBadges.Converters;
using Library.Models.Vc;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Library.Models.OpenBadges
{
    /// <summary>
    /// A collection of information about the recipient of an achievement. Maps to Credential Subject in [[VC-DATA-MODEL-2.0]].
    /// </summary>
    [JsonConverter(typeof(AchievementSubjectConverter))]
    public partial class AchievementSubject : CredentialSubject
    {

        /// <summary>
        /// The achievement being asserted.
        /// </summary>
        [Display(Description = "The achievement being asserted.")]
        [JsonPropertyName("achievement")]
        [Required]
        public Achievement? Achievement { get; set; }

        /// <summary>
        /// The datetime the activity ended.
        /// </summary>
        [Display(Description = "The datetime the activity ended.")]
        [JsonPropertyName("activityEndDate")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTimeOffset? ActivityEndDate { get; set; }

        /// <summary>
        /// The datetime the activity started.
        /// </summary>
        [Display(Description = "The datetime the activity started.")]
        [JsonPropertyName("activityStartDate")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTimeOffset? ActivityStartDate { get; set; }

        /// <summary>
        /// The number of credits earned, generally in semester or quarter credit hours. This field correlates with the Achievement `creditsAvailable` field.
        /// </summary>
        [Display(Description = "The number of credits earned, generally in semester or quarter credit hours. This field correlates with the Achievement `creditsAvailable` field.")]
        [JsonPropertyName("creditsEarned")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? CreditsEarned { get; set; }

        /// <summary>
        /// The identity of the credential subject.
        /// </summary>
        /// <remarks>
        /// Within AchievementSubject the `id` property is not required if at least one `identifier` is provided. If both are provided, they must refer to the same entity. The `id` property is expected to be a URI or a blank node identifier that uniquely identifies the credential subject, while `identifier` can include various types of identifiers such as email addresses, phone numbers, or other forms of identification.
        /// </remarks>
        [JsonPropertyName("id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public override string? Id { get; set; }

        private ICollection<IdentityObject> _identifier = new Collection<IdentityObject>();
        /// <summary>
        /// Other identifiers for the recipient of the achievement. Either `id` or at least one `identifier` MUST be supplied.
        /// </summary>
        [Display(Description = "Other identifiers for the recipient of the achievement. Either `id` or at least one `identifier` MUST be supplied.")]
        [JsonPropertyName("identifier")]
        [JsonConverter(typeof(IdentityObjectConverter))]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ICollection<IdentityObject>? Identifier
        {
            get => _identifier.Count == 0 ? null : _identifier;
            set => _identifier = value ?? new Collection<IdentityObject>();
        }

        /// <summary>
        /// An image representing this user's achievement. If present, this must be a PNG or SVG image, and should be prepared via the 'baking' instructions. An 'unbaked' image for the achievement is defined in the Achievement class and should not be duplicated here.
        /// </summary>
        [Display(Description = "An image representing this user's achievement. If present, this must be a PNG or SVG image, and should be prepared via the 'baking' instructions. An 'unbaked' image for the achievement is defined in the Achievement class and should not be duplicated here.")]
        [JsonPropertyName("image")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Image? Image { get; set; }

        /// <summary>
        /// The license number that was issued with this credential.
        /// </summary>
        [Display(Description = "The license number that was issued with this credential.")]
        [JsonPropertyName("licenseNumber")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? LicenseNumber { get; set; }

        /// <summary>
        /// A narrative that connects multiple pieces of evidence. Likely only present at this location if evidence is a multi-value array.
        /// </summary>
        [Display(Description = "A narrative that connects multiple pieces of evidence. Likely only present at this location if evidence is a multi-value array.")]
        [JsonPropertyName("narrative")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Narrative { get; set; }

        private ICollection<Result> _result = new Collection<Result>();
        /// <summary>
        /// The set of results being asserted.
        /// </summary>
        [Display(Description = "The set of results being asserted.")]
        [JsonPropertyName("result")]
        [JsonConverter(typeof(ResultConverter))]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ICollection<Result>? Result
        {
            get => _result.Count == 0 ? null : _result;
            set => _result = value ?? new Collection<Result>();
        }

        /// <summary>
        /// Role, position, or title of the learner when demonstrating or performing the achievement or evidence of learning being asserted. Examples include 'Student President', 'Intern', 'Captain', etc.
        /// </summary>
        [Display(Description = "Role, position, or title of the learner when demonstrating or performing the achievement or evidence of learning being asserted. Examples include 'Student President', 'Intern', 'Captain', etc.")]
        [JsonPropertyName("role")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Role { get; set; }

        /// <summary>
        /// The source of the achievement.
        /// </summary>
        [Display(Description = "The source of the achievement.")]
        [JsonPropertyName("source")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Profile? Source { get; set; }

        /// <summary>
        /// The academic term in which this assertion was achieved.
        /// </summary>
        [Display(Description = "The academic term in which this assertion was achieved.")]
        [JsonPropertyName("term")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Term { get; set; }

        /// <summary>
        /// The value of the type property MUST be an unordered set. One of the items MUST be the term 'AchievementSubject'.
        /// </summary>
        [Display(Description = "The value of the type property MUST be an unordered set. One of the items MUST be the term 'AchievementSubject'.")]
        [JsonPropertyName("type")]
        [Required]
        [MinLength(1)]
        [JsonConverter(typeof(StringCollectionConverter))]
        public override ICollection<string>? Type { get; set; } = new Collection<string>();
    }
}