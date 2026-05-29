namespace Library.Models.OpenBadges.Converters;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Converts ResultType to/from JSON as a string value.
/// </summary>
public class ResultTypeConverter : JsonConverter<ResultType>
{
    public override ResultType? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException("ResultType must be a string.");

        var typeValue = reader.GetString();
        if (string.IsNullOrEmpty(typeValue))
            throw new JsonException("ResultType cannot be null or empty.");

        return new ResultType { Type = typeValue };
    }

    public override void Write(Utf8JsonWriter writer, ResultType value, JsonSerializerOptions options)
    {
        if (value == null)
            throw new JsonException("ResultType cannot be null.");

        writer.WriteStringValue(value.Type);
    }
}