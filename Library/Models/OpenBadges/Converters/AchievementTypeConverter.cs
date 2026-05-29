namespace Library.Models.OpenBadges.Converters;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Converts AchievementType to/from JSON as a string value.
/// </summary>
public class AchievementTypeConverter : JsonConverter<AchievementType>
{
    public override AchievementType? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException("AchievementType must be a string.");

        var typeValue = reader.GetString();
        if (string.IsNullOrEmpty(typeValue))
            throw new JsonException("AchievementType cannot be null or empty.");

        return new AchievementType { Type = typeValue };
    }

    public override void Write(Utf8JsonWriter writer, AchievementType value, JsonSerializerOptions options)
    {
        if (value == null)
            throw new JsonException("AchievementType cannot be null.");

        writer.WriteStringValue(value.Type);
    }
}