namespace Library.Models.Vc.Converters;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Converts DataIntegrityProof collection to/from JSON, handling single value or array.
/// </summary>
public class ProofConverter : JsonConverter<ICollection<DataIntegrityProof>>
{
    public override ICollection<DataIntegrityProof>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            var list = new Collection<DataIntegrityProof>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                    break;

                var item = JsonSerializer.Deserialize<DataIntegrityProof>(ref reader, options);
                if (item != null)
                    list.Add(item);
            }
            return list;
        }
        else if (reader.TokenType == JsonTokenType.StartObject)
        {
            var item = JsonSerializer.Deserialize<DataIntegrityProof>(ref reader, options);
            return item != null ? new Collection<DataIntegrityProof> { item } : new Collection<DataIntegrityProof>();
        }

        throw new JsonException("Expected array or object for proof property.");
    }

    public override void Write(Utf8JsonWriter writer, ICollection<DataIntegrityProof> value, JsonSerializerOptions options)
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