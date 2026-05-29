namespace Library.Models.OpenBadges
{
    using Library.Models.OpenBadges.Converters;
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json.Serialization;

    /// <summary>
    /// The type of achievement, for example 'Award' or 'Certification'. This is an extensible enumerated vocabulary. 
    /// Extending the vocabulary makes use of a naming convention where new terms must start with the substring "ext:".
    /// </summary>
    [JsonConverter(typeof(AchievementTypeConverter))]
    public partial class AchievementType : IComparable<AchievementType>
    {
        /// <summary>
        /// The achievement type value. Can be one of the predefined terms or a custom term starting with "ext:".
        /// </summary>
        [JsonPropertyName("type")]
        [Required]
        public required string Type { get; set; }

        /// <summary>
        /// Predefined achievement type terms.
        /// </summary>
        public static class Terms
        {
            public const string Achievement = "Achievement";
            public const string ApprenticeshipCertificate = "ApprenticeshipCertificate";
            public const string Assessment = "Assessment";
            public const string Assignment = "Assignment";
            public const string AssociateDegree = "AssociateDegree";
            public const string Award = "Award";
            public const string Badge = "Badge";
            public const string BachelorDegree = "BachelorDegree";
            public const string Certificate = "Certificate";
            public const string CertificateOfCompletion = "CertificateOfCompletion";
            public const string Certification = "Certification";
            public const string CommunityService = "CommunityService";
            public const string Competency = "Competency";
            public const string Course = "Course";
            public const string CoCurricular = "CoCurricular";
            public const string Degree = "Degree";
            public const string Diploma = "Diploma";
            public const string DoctoralDegree = "DoctoralDegree";
            public const string Fieldwork = "Fieldwork";
            public const string GeneralEducationDevelopment = "GeneralEducationDevelopment";
            public const string JourneymanCertificate = "JourneymanCertificate";
            public const string LearningProgram = "LearningProgram";
            public const string License = "License";
            public const string Membership = "Membership";
            public const string ProfessionalDoctorate = "ProfessionalDoctorate";
            public const string QualityAssuranceCredential = "QualityAssuranceCredential";
            public const string MasterCertificate = "MasterCertificate";
            public const string MasterDegree = "MasterDegree";
            public const string MicroCredential = "MicroCredential";
            public const string ResearchDoctorate = "ResearchDoctorate";
            public const string SecondarySchoolDiploma = "SecondarySchoolDiploma";
        }

        /// <summary>
        /// Creates an AchievementType from a type string.
        /// </summary>
        /// <param name="type">The achievement type value.</param>
        /// <returns>An AchievementType instance.</returns>
        public static AchievementType FromType(string type) => new() { Type = type };

        /// <summary>
        /// Validates that custom terms start with "ext:".
        /// </summary>
        /// <returns>True if the type is valid.</returns>
        public bool IsValid()
        {
            // Predefined terms are always valid
            if (IsPredefinedTerm(Type))
                return true;

            // Custom terms must start with "ext:"
            return Type.StartsWith("ext:", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsPredefinedTerm(string type)
        {
            return type switch
            {
                Terms.Achievement or
                Terms.ApprenticeshipCertificate or
                Terms.Assessment or
                Terms.Assignment or
                Terms.AssociateDegree or
                Terms.Award or
                Terms.Badge or
                Terms.BachelorDegree or
                Terms.Certificate or
                Terms.CertificateOfCompletion or
                Terms.Certification or
                Terms.CommunityService or
                Terms.Competency or
                Terms.Course or
                Terms.CoCurricular or
                Terms.Degree or
                Terms.Diploma or
                Terms.DoctoralDegree or
                Terms.Fieldwork or
                Terms.GeneralEducationDevelopment or
                Terms.JourneymanCertificate or
                Terms.LearningProgram or
                Terms.License or
                Terms.Membership or
                Terms.ProfessionalDoctorate or
                Terms.QualityAssuranceCredential or
                Terms.MasterCertificate or
                Terms.MasterDegree or
                Terms.MicroCredential or
                Terms.ResearchDoctorate or
                Terms.SecondarySchoolDiploma => true,
                _ => false
            };
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object based on the Type property.
        /// </summary>
        public override bool Equals(object? obj)
        {
            if (obj is AchievementType other)
            {
                return Type == other.Type;
            }
            return false;
        }

        /// <summary>
        /// Returns a hash code for the current object based on the Type property.
        /// </summary>
        public override int GetHashCode()
        {
            return Type?.GetHashCode() ?? 0;
        }

        /// <summary>
        /// Equality operator for AchievementType.
        /// </summary>
        public static bool operator ==(AchievementType? left, AchievementType? right)
        {
            if (left is null)
                return right is null;
            return left.Equals(right);
        }

        /// <summary>
        /// Inequality operator for AchievementType.
        /// </summary>
        public static bool operator !=(AchievementType? left, AchievementType? right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Compares the current instance with another AchievementType based on the Type property.
        /// </summary>
        public int CompareTo(AchievementType? other)
        {
            if (other is null) return 1;
            return string.Compare(Type, other.Type, StringComparison.OrdinalIgnoreCase);
        }
    }
}