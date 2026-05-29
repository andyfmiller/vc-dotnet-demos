namespace Library.Models.Vc.Converters
{
    using Library.Models.Vc;
    using System.Collections.ObjectModel;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Factory for creating CredentialSubjectConverter instances for generic TSubject types.
    /// </summary>
    public class CredentialSubjectConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            if (!typeToConvert.IsGenericType)
                return false;

            var genericType = typeToConvert.GetGenericTypeDefinition();
            if (genericType != typeof(ICollection<>))
                return false;

            var elementType = typeToConvert.GetGenericArguments()[0];
            return typeof(CredentialSubject).IsAssignableFrom(elementType);
        }

        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var subjectType = typeToConvert.GetGenericArguments()[0];
            var converterType = typeof(CredentialSubjectConverter<>).MakeGenericType(subjectType);
            return (JsonConverter?)Activator.CreateInstance(converterType);
        }
    }

    /// <summary>
    /// Custom JSON converter for CredentialSubject that handles both single objects and arrays
    /// according to the Verifiable Credentials Data Model 2.0 spec.
    /// </summary>
    public class CredentialSubjectConverter<TSubject> : JsonConverter<ICollection<TSubject>>
        where TSubject : CredentialSubject
    {
        // Tell System.Text.Json to call this converter even for null values
        public override bool HandleNull => true;

        public override ICollection<TSubject> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var subjects = new Collection<TSubject>();

            // Handle null token - return empty collection
            if (reader.TokenType == JsonTokenType.Null)
            {
                return subjects;
            }

            if (reader.TokenType == JsonTokenType.StartObject)
            {
                // Single subject as object
                var subject = JsonSerializer.Deserialize<TSubject>(ref reader, options);
                if (subject is not null)
                    subjects.Add(subject);
            }
            else if (reader.TokenType == JsonTokenType.StartArray)
            {
                // Multiple subjects as array
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndArray)
                        break;

                    if (reader.TokenType == JsonTokenType.StartObject)
                    {
                        var subject = JsonSerializer.Deserialize<TSubject>(ref reader, options);
                        if (subject is not null)
                            subjects.Add(subject);
                    }
                }
            }

            return subjects;
        }

        public override void Write(Utf8JsonWriter writer, ICollection<TSubject> value, JsonSerializerOptions options)
        {
            if (value == null || value.Count == 0)
            {
                writer.WriteNullValue(); // Must write something!
                return;
            }

            // If there's only one subject, serialize as a single object
            if (value.Count == 1)
            {
                JsonSerializer.Serialize(writer, value.First(), options);
            }
            else
            {
                // Multiple subjects, serialize as array
                writer.WriteStartArray();
                foreach (var subject in value)
                {
                    JsonSerializer.Serialize(writer, subject, options);
                }
                writer.WriteEndArray();
            }
        }
    }

    // Extension method to get First from ICollection
    internal static class CollectionExtensions
    {
        public static T First<T>(this ICollection<T> collection)
        {
            foreach (var item in collection)
                return item;
            throw new InvalidOperationException("Collection is empty");
        }
    }
}