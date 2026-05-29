namespace Library.Models.OpenBadges.Converters;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Converts AlignmentTargetType to/from JSON as a string value.
/// </summary>
public class AlignmentTargetTypeConverter : JsonConverter<AlignmentTargetType>
{
    public override AlignmentTargetType? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException("AlignmentTargetType must be a string.");

        var typeValue = reader.GetString();
        if (string.IsNullOrEmpty(typeValue))
            throw new JsonException("AlignmentTargetType cannot be null or empty.");

        return new AlignmentTargetType { Type = typeValue };
    }

    public override void Write(Utf8JsonWriter writer, AlignmentTargetType value, JsonSerializerOptions options)
    {
        if (value == null)
            throw new JsonException("AlignmentTargetType cannot be null.");

        writer.WriteStringValue(value.Type);
    }
}