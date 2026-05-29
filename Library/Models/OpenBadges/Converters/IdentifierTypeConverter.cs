namespace Library.Models.OpenBadges.Converters;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

// Converts IdentifierType to/from JSON as a string value.
public class IdentifierTypeConverter : JsonConverter<IdentifierType>
{
    public override IdentifierType? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException("IdentifierType must be a string.");

        var typeValue = reader.GetString();
        if (string.IsNullOrEmpty(typeValue))
            throw new JsonException("IdentifierType cannot be null or empty.");

        return new IdentifierType { Type = typeValue };
    }

    public override void Write(Utf8JsonWriter writer, IdentifierType value, JsonSerializerOptions options)
    {
        if (value == null)
            throw new JsonException("IdentifierType cannot be null.");

        writer.WriteStringValue(value.Type);
    }
}