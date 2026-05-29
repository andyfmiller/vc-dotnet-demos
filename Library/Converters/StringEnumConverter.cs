using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Library.Converters
{
    public class StringEnumConverter<T> : JsonConverter<T>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            if (typeToConvert.IsEnum)
            {
                return true;
            }

            return base.CanConvert(typeToConvert);
        }

        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (typeToConvert.IsEnum)
            {
                var value = reader.GetString();

                if (value != null)
                {
                    var memberInfos = typeToConvert
                        .GetMembers(BindingFlags.Public | BindingFlags.Static);

                    foreach (var memberInfo in memberInfos)
                    {
                        var attribute = memberInfo
                            .GetCustomAttributes(typeof(EnumMemberAttribute), false)
                            .FirstOrDefault() as EnumMemberAttribute;

                        if (attribute != null && attribute.Value == value)
                        {
                            return (T)Enum.Parse(typeToConvert, memberInfo.Name, false);
                        }
                    }

                    if (Enum.TryParse(typeToConvert, value, false, out var result))
                    {
                        return (T)result;
                    }

                    return default;
                }
            }

            return default;
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
            }
            else
            {
                if (typeof(T).IsEnum)
                {
                    var content = $"{value}";

                    // Look for EnumMemberAttributes

                    var memberInfo = typeof(T)
                        .GetMember(content)
                        .FirstOrDefault();

                    if (memberInfo != null)
                    {
                        var attribute = memberInfo
    .GetCustomAttributes(typeof(EnumMemberAttribute), false)
    .FirstOrDefault() as EnumMemberAttribute;

                        if (attribute != null)
                        {
                            content = attribute.Value;
                        }
                    }

                    writer.WriteStringValue(content);
                }
            }
        }
    }
}
