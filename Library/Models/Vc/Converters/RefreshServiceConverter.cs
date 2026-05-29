namespace Library.Models.Vc.Converters
{
    using System.Collections.ObjectModel;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Custom JSON converter for RefreshService that handles both single objects and arrays
    /// according to the Verifiable Credentials Data Model 2.0 spec.
    /// </summary>
    public class RefreshServiceConverter : JsonConverter<ICollection<RefreshService>?>
    {
        public override ICollection<RefreshService>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Handle null/missing token - return null so property can be omitted
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            var services = new Collection<RefreshService>();

            if (reader.TokenType == JsonTokenType.StartObject)
            {
                // Single service as object
                var service = JsonSerializer.Deserialize<RefreshService>(ref reader, options);
                if (service is not null)
                    services.Add(service);
            }
            else if (reader.TokenType == JsonTokenType.StartArray)
            {
                // Multiple services as array
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndArray)
                        break;

                    if (reader.TokenType == JsonTokenType.StartObject)
                    {
                        var service = JsonSerializer.Deserialize<RefreshService>(ref reader, options);
                        if (service is not null)
                            services.Add(service);
                    }
                }
            }

            return services.Count > 0 ? services : null;
        }

        public override void Write(Utf8JsonWriter writer, ICollection<RefreshService>? value, JsonSerializerOptions options)
        {
            // Return null for empty/null collections - JsonIgnore will omit it
            if (value == null || value.Count == 0)
            {
                writer.WriteNullValue();
                return;
            }

            // If there's only one service, serialize as a single object
            if (value.Count == 1)
            {
                JsonSerializer.Serialize(writer, value.First(), options);
            }
            else
            {
                // Multiple services, serialize as array
                writer.WriteStartArray();
                foreach (var service in value)
                {
                    JsonSerializer.Serialize(writer, service, options);
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