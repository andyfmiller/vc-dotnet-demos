namespace Library.Models.Vc.Converters
{
    using System.Collections.ObjectModel;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Custom JSON converter for TermsOfUse that handles both single objects and arrays
    /// according to the Verifiable Credentials Data Model 2.0 spec.
    /// </summary>
    public class TermsOfUseConverter : JsonConverter<ICollection<TermsOfUse>?>
    {
        public override ICollection<TermsOfUse>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Handle null/missing token - return null so property can be omitted
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            var termsOfUse = new Collection<TermsOfUse>();

            if (reader.TokenType == JsonTokenType.StartObject)
            {
                // Single terms of use as object
                var term = JsonSerializer.Deserialize<TermsOfUse>(ref reader, options);
                if (term is not null)
                    termsOfUse.Add(term);
            }
            else if (reader.TokenType == JsonTokenType.StartArray)
            {
                // Multiple terms of use as array
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndArray)
                        break;

                    if (reader.TokenType == JsonTokenType.StartObject)
                    {
                        var term = JsonSerializer.Deserialize<TermsOfUse>(ref reader, options);
                        if (term is not null)
                            termsOfUse.Add(term);
                    }
                }
            }

            return termsOfUse.Count > 0 ? termsOfUse : null;
        }

        public override void Write(Utf8JsonWriter writer, ICollection<TermsOfUse>? value, JsonSerializerOptions options)
        {
            // Return null for empty/null collections - JsonIgnore will omit it
            if (value == null || value.Count == 0)
            {
                writer.WriteNullValue();
                return;
            }

            // If there's only one term, serialize as a single object
            if (value.Count == 1)
            {
                JsonSerializer.Serialize(writer, value.First(), options);
            }
            else
            {
                // Multiple terms, serialize as array
                writer.WriteStartArray();
                foreach (var term in value)
                {
                    JsonSerializer.Serialize(writer, term, options);
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