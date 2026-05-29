namespace Library.Models.Vc.Converters
{
    using System.Collections.ObjectModel;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Custom JSON converter for Evidence that handles both single objects and arrays
    /// according to the Verifiable Credentials Data Model 2.0 spec.
    /// </summary>
    public class EvidenceConverter : JsonConverter<ICollection<Evidence>?>
    {
        public override ICollection<Evidence>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Handle null/missing token - return null so property can be omitted
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            // Use JsonElement to consume the value and break the converter chain
            var element = JsonElement.ParseValue(ref reader);

            var evidence = new Collection<Evidence>();

            if (element.ValueKind == JsonValueKind.Object)
            {
                // Single evidence as object - deserialize directly from element
                var item = element.Deserialize<Evidence>(options);
                if (item is not null)
                    evidence.Add(item);
            }
            else if (element.ValueKind == JsonValueKind.Array)
            {
                // Multiple evidence as array
                foreach (var arrayElement in element.EnumerateArray())
                {
                    if (arrayElement.ValueKind == JsonValueKind.Object)
                    {
                        var item = arrayElement.Deserialize<Evidence>(options);
                        if (item is not null)
                            evidence.Add(item);
                    }
                }
            }

            return evidence.Count > 0 ? evidence : null;
        }

        public override void Write(Utf8JsonWriter writer, ICollection<Evidence>? value, JsonSerializerOptions options)
        {
            // Return null for empty/null collections - JsonIgnore will omit it
            if (value == null || value.Count == 0)
            {
                writer.WriteNullValue();
                return;
            }

            // If there's only one evidence, serialize as a single object (not array)
            if (value.Count == 1)
            {
                JsonSerializer.Serialize(writer, value.First(), options);
            }
            else
            {
                // Multiple evidence, serialize as array
                JsonSerializer.Serialize(writer, value, options);
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