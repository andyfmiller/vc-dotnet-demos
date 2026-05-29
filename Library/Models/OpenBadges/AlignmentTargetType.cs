namespace Library.Models.OpenBadges
{
    using Library.Models.OpenBadges.Converters;
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json.Serialization;

    /// <summary>
    /// The type of the alignment target node in the target framework. This is an extensible enumerated vocabulary. 
    /// Extending the vocabulary makes use of a naming convention where new terms must start with the substring "ext:".
    /// </summary>
    [JsonConverter(typeof(AlignmentTargetTypeConverter))]
    public partial class AlignmentTargetType
    {
        /// <summary>
        /// The alignment target type value. Can be one of the predefined terms or a custom term starting with "ext:".
        /// </summary>
        [JsonPropertyName("type")]
        [Required]
        public required string Type { get; set; }

        /// <summary>
        /// Predefined alignment target type terms.
        /// </summary>
        public static class Terms
        {
            /// <summary>
            /// An alignment to a CTDL-ASN/CTDL competency published by Credential Engine.
            /// </summary>
            public const string CeasnCompetency = "ceasn:Competency";

            /// <summary>
            /// An alignment to a CTDL Credential published by Credential Engine.
            /// </summary>
            public const string CetermsCredential = "ceterms:Credential";

            /// <summary>
            /// An alignment to a CASE Framework Item.
            /// </summary>
            public const string CFItem = "CFItem";

            /// <summary>
            /// An alignment to a CASE Framework Rubric.
            /// </summary>
            public const string CFRubric = "CFRubric";

            /// <summary>
            /// An alignment to a CASE Framework Rubric Criterion.
            /// </summary>
            public const string CFRubricCriterion = "CFRubricCriterion";

            /// <summary>
            /// An alignment to a CASE Framework Rubric Criterion Level.
            /// </summary>
            public const string CFRubricCriterionLevel = "CFRubricCriterionLevel";

            /// <summary>
            /// An alignment to a Credential Engine Item.
            /// </summary>
            public const string CTDL = "CTDL";
        }

        /// <summary>
        /// Creates an AlignmentTargetType from a type string.
        /// </summary>
        /// <param name="type">The alignment target type value.</param>
        /// <returns>An AlignmentTargetType instance.</returns>
        public static AlignmentTargetType FromType(string type) => new() { Type = type };

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
                Terms.CeasnCompetency or
                Terms.CetermsCredential or
                Terms.CFItem or
                Terms.CFRubric or
                Terms.CFRubricCriterion or
                Terms.CFRubricCriterionLevel or
                Terms.CTDL => true,
                _ => false
            };
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object based on the Type property.
        /// </summary>
        public override bool Equals(object? obj)
        {
            if (obj is AlignmentTargetType other)
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
        /// Equality operator for AlignmentTargetType.
        /// </summary>
        public static bool operator ==(AlignmentTargetType? left, AlignmentTargetType? right)
        {
            if (left is null)
                return right is null;
            return left.Equals(right);
        }

        /// <summary>
        /// Inequality operator for AlignmentTargetType.
        /// </summary>
        public static bool operator !=(AlignmentTargetType? left, AlignmentTargetType? right)
        {
            return !(left == right);
        }
    }
}