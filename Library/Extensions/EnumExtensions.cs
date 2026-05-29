using System.Runtime.Serialization;

namespace Library.Extensions
{
    public static class EnumExtensions
    {
        /// <summary>
        /// Returns the <see cref="EnumMemberAttribute.Value"/> for an Enum. If the attribute is missing
        /// or the value is null, returns <see cref="Enum.GetName(Type, object)"/>.
        /// </summary>
        /// <param name="value">The Enum value to inspect.</param>
        /// <returns>A string name.</returns>
        /// <remarks>
        /// This is a convenience method for extensible enumerations like <see cref="AchievementDType.AchievementType"/>
        /// which is stored as a string but validated against a schema which has a specific set of
        /// allowed values and a pattern for extended values.
        /// </remarks>
        public static string? GetMemberName(this Enum value)
        {
            var type = value.GetType();
            var name = Enum.GetName(type, value);
            if (name == null) return default;

            var field = type.GetField(name);
            if (field == null) return name;

            //var memberInfo = type
            //    .GetMembers(BindingFlags.Public | BindingFlags.Static)
            //    .Single(x => x.Name == name);

            var attribute = field
                .GetCustomAttributes(typeof(EnumMemberAttribute), false)
                .FirstOrDefault() as EnumMemberAttribute;

            return attribute?.Value ?? name;
        }
    }
}
