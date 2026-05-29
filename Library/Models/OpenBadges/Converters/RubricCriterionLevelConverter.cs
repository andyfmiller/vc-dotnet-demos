namespace Library.Models.OpenBadges.Converters;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Converts RubricCriterionLevel collection to/from JSON, handling single value or array.
/// </summary>
public class RubricCriterionLevelConverter : JsonConverter<ICollection<RubricCriterionLevel>>
{
    public override ICollection<RubricCriterionLevel>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            var list = new Collection<RubricCriterionLevel>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                    break;

                var item = JsonSerializer.Deserialize<RubricCriterionLevel>(ref reader, options);
                if (item != null)
                    list.Add(item);
            }
            return list;
        }
        else if (reader.TokenType == JsonTokenType.StartObject)
        {
            var item = JsonSerializer.Deserialize<RubricCriterionLevel>(ref reader, options);
            return item != null ? new Collection<RubricCriterionLevel> { item } : new Collection<RubricCriterionLevel>();
        }

        throw new JsonException("Expected array or object for rubricCriterionLevel property.");
    }

    public override void Write(Utf8JsonWriter writer, ICollection<RubricCriterionLevel> value, JsonSerializerOptions options)
    {
        if (value == null || value.Count == 0)
        {
            writer.WriteNullValue();
            return;
        }

        if (value.Count == 1)
        {
            JsonSerializer.Serialize(writer, value.First(), options);
        }
        else
        {
            JsonSerializer.Serialize(writer, value, options);
        }
    }
}