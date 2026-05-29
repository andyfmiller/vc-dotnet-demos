using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Library.Models.Vc.Converters
{
    /// <summary>
    /// Custom JSON converter for CredentialStatus that handles both single objects and arrays
    /// according to the Verifiable Credentials Data Model 2.0 spec.
    /// </summary>
    public class CredentialStatusConverter : JsonConverter<ICollection<CredentialStatus>?>
    {
        public override ICollection<CredentialStatus>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var statuses = new Collection<CredentialStatus>();

            // Handle null token - return empty collection
            if (reader.TokenType == JsonTokenType.Null)
            {
                return statuses;
            }

            if (reader.TokenType == JsonTokenType.StartObject)
            {
                // Single status as object
                var status = JsonSerializer.Deserialize<CredentialStatus>(ref reader, options);
                if (status is not null)
                    statuses.Add(status);
            }
            else if (reader.TokenType == JsonTokenType.StartArray)
            {
                // Multiple statuses as array
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndArray)
                        break;

                    if (reader.TokenType == JsonTokenType.StartObject)
                    {
                        var status = JsonSerializer.Deserialize<CredentialStatus>(ref reader, options);
                        if (status is not null)
                            statuses.Add(status);
                    }
                }
            }

            return statuses;
        }

        public override void Write(Utf8JsonWriter writer, ICollection<CredentialStatus>? value, JsonSerializerOptions options)
        {
            // Converter receives null for empty collections (handled by property getter)
            // JsonIgnore will omit null values
            if (value == null || value.Count == 0)
            {
                writer.WriteNullValue();
                return;
            }

            // If there's only one status, serialize as a single object
            if (value.Count == 1)
            {
                JsonSerializer.Serialize(writer, value.First(), options);
            }
            else
            {
                // Multiple statuses, serialize as array
                writer.WriteStartArray();
                foreach (var status in value)
                {
                    JsonSerializer.Serialize(writer, status, options);
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