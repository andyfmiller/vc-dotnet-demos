namespace Library.Models.Vc.Converters
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Serializes DateTimeOffset values as UTC ISO 8601 strings (e.g. "2024-01-01T00:00:00Z").
    /// Required by OB3/VC Data Model — the 1EdTech validator rejects timestamps with local offsets.
    /// </summary>
    internal class DateTimeOffsetUtcConverter : JsonConverter<DateTimeOffset>
    {
        public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var s = reader.GetString();
            if (s == null) throw new JsonException("Expected a date-time string.");
            return DateTimeOffset.Parse(s);
        }

        public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"));
        }
    }
}
