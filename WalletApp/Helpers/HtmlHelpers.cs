using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace WalletApp.Helpers
{
    public class HtmlHelpers
    {
        /// <summary>
        /// Returns a list of <see cref="SelectListItem"/> where the value of each item is the <see cref="EnumMemberAttribute.Value"/>
        /// for the Enum. If the attribute is missing or the value is null, the list item value will be <see cref="Enum.GetName(Type, object)"/>.
        /// </summary>
        /// <returns>A list of items.</returns>
        /// <remarks>
        /// This is a convenience method for extensible enumerations like <see cref="AchievementDType.AchievementType"/>
        /// which is stored as a string but validated against a schema which has a specific set of
        /// allowed values and a pattern for extended values.
        /// </remarks>
        public static IList<SelectListItem> GetEnumMemberNamesSelectList<TEnum>() where TEnum : struct
        {
            var list = new List<SelectListItem>();

            foreach (var value in (TEnum[])Enum.GetValues(typeof(TEnum)))
            {
                var type = value.GetType();
                var name = Enum.GetName(type, value);
                if (name is null) continue;

                var field = type.GetField(name);
                if (field is null) continue;

                var memberName = (EnumMemberAttribute?)field
                    .GetCustomAttributes(typeof(EnumMemberAttribute), false)
                    .FirstOrDefault();

                var display = (DisplayAttribute?)field
                    .GetCustomAttributes(typeof(DisplayAttribute), false)
                    .FirstOrDefault();

                list.Add(new SelectListItem(display?.Name ?? name, memberName?.Value ?? name));
            }

            return list;
        }
    }
}
