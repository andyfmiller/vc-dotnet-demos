namespace Library.Models.Vc.Converters
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Serializes nullable DateTimeOffset values as UTC ISO 8601 strings (e.g. "2024-01-01T00:00:00Z"),
    /// or as JSON null when the value is null.
    /// </summary>
    internal class NullableDateTimeOffsetUtcConverter : JsonConverter<DateTimeOffset?>
    {
        public override DateTimeOffset? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            var s = reader.GetString();
            if (s == null) return null;
            return DateTimeOffset.Parse(s);
        }

        public override void Write(Utf8JsonWriter writer, DateTimeOffset? value, JsonSerializerOptions options)
        {
            if (value is null)
                writer.WriteNullValue();
            else
                writer.WriteStringValue(value.Value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"));
        }
    }
}
