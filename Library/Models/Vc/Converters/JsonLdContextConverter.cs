using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Library.Models.Vc.Converters
{
    public class JsonLdContextConverter : JsonConverter<ICollection<object>>
    {
        public override ICollection<object> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var contexts = new Collection<object>();

            if (reader.TokenType == JsonTokenType.String)
            {
                // Single string context
                var str = reader.GetString();
                if (str is not null)
                    contexts.Add(str);
            }
            else if (reader.TokenType == JsonTokenType.StartArray)
            {
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndArray)
                        break;

                    if (reader.TokenType == JsonTokenType.String)
                    {
                        var str = reader.GetString();
                        if (str is not null)
                            contexts.Add(str);
                    }
                    else if (reader.TokenType == JsonTokenType.StartObject)
                    {
                        // Parse context object and preserve as JsonElement
                        using var doc = JsonDocument.ParseValue(ref reader);
                        contexts.Add(doc.RootElement.Clone());
                    }
                }
            }
            else if (reader.TokenType == JsonTokenType.StartObject)
            {
                // Parse context object and preserve as JsonElement
                using var doc = JsonDocument.ParseValue(ref reader);
                contexts.Add(doc.RootElement.Clone());
            }

            return contexts;
        }

        public override void Write(Utf8JsonWriter writer, ICollection<object> value, JsonSerializerOptions options)
        {
            if (value == null || value.Count == 0)
            {
                writer.WriteNullValue();
                return;
            }

            // JSON-LD contexts should always be arrays (even with one item) per spec
            writer.WriteStartArray();

            foreach (var item in value)
            {
                if (item is string str)
                {
                    writer.WriteStringValue(str);
                }
                else
                {
                    JsonSerializer.Serialize(writer, item, options);
                }
            }

            writer.WriteEndArray();
        }
    }
}