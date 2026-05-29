namespace Library.Models.OpenBadges
{
    using Library.Models.OpenBadges.Converters;
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json.Serialization;

    /// <summary>
    /// The type of result. This is an extensible enumerated vocabulary. 
    /// Extending the vocabulary makes use of a naming convention where new terms must start with the substring "ext:".
    /// </summary>
    [JsonConverter(typeof(ResultTypeConverter))]
    public partial class ResultType
    {
        /// <summary>
        /// The result type value. Can be one of the predefined terms or a custom term starting with "ext:".
        /// </summary>
        [JsonPropertyName("type")]
        [Required]
        public required string Type { get; set; }

        /// <summary>
        /// Predefined result type terms.
        /// </summary>
        public static class Terms
        {
            public const string GradePointAverage = "GradePointAverage";
            public const string LetterGrade = "LetterGrade";
            public const string Percent = "Percent";
            public const string PerformanceLevel = "PerformanceLevel";
            public const string PredictedScore = "PredictedScore";
            public const string RawScore = "RawScore";
            public const string Result = "Result";
            public const string RubricCriterion = "RubricCriterion";
            public const string RubricCriterionLevel = "RubricCriterionLevel";
            public const string RubricScore = "RubricScore";
            public const string ScaledScore = "ScaledScore";
            public const string Status = "Status";
        }

        /// <summary>
        /// Creates a ResultType from a type string.
        /// </summary>
        /// <param name="type">The result type value.</param>
        /// <returns>A ResultType instance.</returns>
        public static ResultType FromType(string type) => new() { Type = type };

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
                Terms.GradePointAverage or
                Terms.LetterGrade or
                Terms.Percent or
                Terms.PerformanceLevel or
                Terms.PredictedScore or
                Terms.RawScore or
                Terms.Result or
                Terms.RubricCriterion or
                Terms.RubricCriterionLevel or
                Terms.RubricScore or
                Terms.ScaledScore or
                Terms.Status => true,
                _ => false
            };
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object based on the Type property.
        /// </summary>
        public override bool Equals(object? obj)
        {
            if (obj is ResultType other)
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
        /// Equality operator for ResultType.
        /// </summary>
        public static bool operator ==(ResultType? left, ResultType? right)
        {
            if (left is null)
                return right is null;
            return left.Equals(right);
        }

        /// <summary>
        /// Inequality operator for ResultType.
        /// </summary>
        public static bool operator !=(ResultType? left, ResultType? right)
        {
            return !(left == right);
        }
    }
}