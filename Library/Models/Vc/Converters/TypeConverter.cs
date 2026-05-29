namespace Library.Models.Vc.Converters
{
    using System.Collections.ObjectModel;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Custom JSON converter for Type properties that handles both single strings and arrays
    /// according to the Verifiable Credentials Data Model 2.0 spec (derived from JSON-LD).
    /// A single type is serialized as a string, multiple types as an array.
    /// </summary>
    public class TypeConverter : JsonConverter<ICollection<string>>
    {
        public override ICollection<string> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var types = new Collection<string>();

            if (reader.TokenType == JsonTokenType.String)
            {
                // Single type as string
                var typeValue = reader.GetString();
                if (!string.IsNullOrEmpty(typeValue))
                    types.Add(typeValue);
            }
            else if (reader.TokenType == JsonTokenType.StartArray)
            {
                // Multiple types as array
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndArray)
                        break;

                    if (reader.TokenType == JsonTokenType.String)
                    {
                        var typeValue = reader.GetString();
                        if (!string.IsNullOrEmpty(typeValue))
                            types.Add(typeValue);
                    }
                }
            }

            return types;
        }

        public override void Write(Utf8JsonWriter writer, ICollection<string> value, JsonSerializerOptions options)
        {
            // Return null for empty/null collections - JsonIgnore will omit it
            if (value == null || value.Count == 0)
            {
                writer.WriteNullValue();
                return;
            }

            // Always serialize as array — OB3/VC spec requires array form for 'type'.
            writer.WriteStartArray();
            foreach (var type in value)
            {
                writer.WriteStringValue(type);
            }
            writer.WriteEndArray();
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