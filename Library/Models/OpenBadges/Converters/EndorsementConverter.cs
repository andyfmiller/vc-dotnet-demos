namespace Library.Models.OpenBadges.Converters;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Converts EndorsementCredential collection to/from JSON, handling single value or array.
/// </summary>
public class EndorsementConverter : JsonConverter<ICollection<EndorsementCredential>>
{
    public override ICollection<EndorsementCredential>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return new Collection<EndorsementCredential>();
        }
        
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            var list = new Collection<EndorsementCredential>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                    break;

                var item = JsonSerializer.Deserialize<EndorsementCredential>(ref reader, options);
                if (item != null)
                    list.Add(item);
            }
            return list;
        }
        else if (reader.TokenType == JsonTokenType.StartObject)
        {
            var item = JsonSerializer.Deserialize<EndorsementCredential>(ref reader, options);
            return item != null ? new Collection<EndorsementCredential> { item } : new Collection<EndorsementCredential>();
        }

        throw new JsonException("Expected array or object for endorsement property.");
    }

    public override void Write(Utf8JsonWriter writer, ICollection<EndorsementCredential> value, JsonSerializerOptions options)
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