namespace Library.Models.OpenBadges
{
    using Library.Models.OpenBadges.Converters;
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json.Serialization;

    /// <summary>
    /// The type of identifier. This is an extensible enumerated vocabulary. 
    /// Extending the vocabulary makes use of a naming convention where new terms must start with the substring "ext:".
    /// </summary>
    [JsonConverter(typeof(IdentifierTypeConverter))]
    public partial class IdentifierType
    {
        /// <summary>
        /// The identifier type value. Can be one of the predefined terms or a custom term starting with "ext:".
        /// </summary>
        [JsonPropertyName("type")]
        [Required]
        public required string Type { get; set; }

        /// <summary>
        /// Predefined identifier type terms.
        /// </summary>
        public static class Terms
        {
            public const string Name = "name";
            public const string SourcedId = "sourcedId";
            public const string SystemId = "systemId";
            public const string ProductId = "productId";
            public const string UserName = "userName";
            public const string AccountId = "accountId";
            public const string EmailAddress = "emailAddress";
            public const string NationalIdentityNumber = "nationalIdentityNumber";
            public const string Isbn = "isbn";
            public const string Issn = "issn";
            public const string LisSourcedId = "lisSourcedId";
            public const string OneRosterSourcedId = "oneRosterSourcedId";
            public const string SisSourcedId = "sisSourcedId";
            public const string LtiContextId = "ltiContextId";
            public const string LtiDeploymentId = "ltiDeploymentId";
            public const string LtiToolId = "ltiToolId";
            public const string LtiPlatformId = "ltiPlatformId";
            public const string LtiUserId = "ltiUserId";
            public const string Identifier = "identifier";
        }

        /// <summary>
        /// Creates an IdentifierType from a type string.
        /// </summary>
        /// <param name="type">The identifier type value.</param>
        /// <returns>An IdentifierType instance.</returns>
        public static IdentifierType FromType(string type) => new() { Type = type };

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
                Terms.Name or
                Terms.SourcedId or
                Terms.SystemId or
                Terms.ProductId or
                Terms.UserName or
                Terms.AccountId or
                Terms.EmailAddress or
                Terms.NationalIdentityNumber or
                Terms.Isbn or
                Terms.Issn or
                Terms.LisSourcedId or
                Terms.OneRosterSourcedId or
                Terms.SisSourcedId or
                Terms.LtiContextId or
                Terms.LtiDeploymentId or
                Terms.LtiToolId or
                Terms.LtiPlatformId or
                Terms.LtiUserId or
                Terms.Identifier => true,
                _ => false
            };
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object based on the Type property.
        /// </summary>
        public override bool Equals(object? obj)
        {
            if (obj is IdentifierType other)
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
        /// Equality operator for IdentifierType.
        /// </summary>
        public static bool operator ==(IdentifierType? left, IdentifierType? right)
        {
            if (left is null)
                return right is null;
            return left.Equals(right);
        }

        /// <summary>
        /// Inequality operator for IdentifierType.
        /// </summary>
        public static bool operator !=(IdentifierType? left, IdentifierType? right)
        {
            return !(left == right);
        }
    }
}