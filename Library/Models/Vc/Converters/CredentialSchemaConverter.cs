using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Library.Models.Vc.Converters
{
    /// <summary>
    /// Custom JSON converter for CredentialSchema that handles both single objects and arrays
    /// according to the Verifiable Credentials Data Model 2.0 spec.
    /// </summary>
    public class CredentialSchemaConverter : JsonConverter<ICollection<CredentialSchema>?>
    {
        public override ICollection<CredentialSchema>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Handle null/missing token - return null so property can be omitted
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            var schemas = new Collection<CredentialSchema>();

            if (reader.TokenType == JsonTokenType.StartObject)
            {
                // Single schema as object
                var schema = JsonSerializer.Deserialize<CredentialSchema>(ref reader, options);
                if (schema is not null)
                    schemas.Add(schema);
            }
            else if (reader.TokenType == JsonTokenType.StartArray)
            {
                // Multiple schemas as array
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndArray)
                        break;

                    if (reader.TokenType == JsonTokenType.StartObject)
                    {
                        var schema = JsonSerializer.Deserialize<CredentialSchema>(ref reader, options);
                        if (schema is not null)
                            schemas.Add(schema);
                    }
                }
            }

            return schemas.Count > 0 ? schemas : null;
        }

        public override void Write(Utf8JsonWriter writer, ICollection<CredentialSchema>? value, JsonSerializerOptions options)
        {
            // Return null for empty/null collections - JsonIgnore will omit it
            if (value == null || value.Count == 0)
            {
                writer.WriteNullValue();
                return;
            }

            // If there's only one schema, serialize as a single object
            if (value.Count == 1)
            {
                JsonSerializer.Serialize(writer, value.First(), options);
            }
            else
            {
                // Multiple schemas, serialize as array
                writer.WriteStartArray();
                foreach (var schema in value)
                {
                    JsonSerializer.Serialize(writer, schema, options);
                }
                writer.WriteEndArray();
            }
        }

        // Extension method to get First from ICollection
        private static T First<T>(ICollection<T> collection)
        {
            foreach (var item in collection)
                return item;
            throw new InvalidOperationException("Collection is empty");
        }
    }
}