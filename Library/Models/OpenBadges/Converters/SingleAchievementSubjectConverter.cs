namespace Library.Models.OpenBadges.Converters;

using Library.Models.OpenBadges;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Custom JSON converter for AchievementCredential.CredentialSubject that handles a single AchievementSubject.
/// Supports reading both single object and array formats (for compatibility) but always writes as a single object.
/// </summary>
public class SingleAchievementSubjectConverter : JsonConverter<AchievementSubject>
{
    public override bool HandleNull => true;

    public override AchievementSubject? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Handle null token
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        // Create new options WITHOUT AchievementSubjectConverter to prevent infinite recursion
        var newOptions = new JsonSerializerOptions(options);
        newOptions.Converters.Clear();
        
        foreach (var converter in options.Converters)
        {
            // Skip both this converter and AchievementSubjectConverter
            if (converter is not AchievementSubjectConverter && converter is not SingleAchievementSubjectConverter)
            {
                newOptions.Converters.Add(converter);
            }
        }

        if (reader.TokenType == JsonTokenType.StartObject)
        {
            // Single subject as object (expected format)
            return DeserializeAchievementSubject(ref reader, newOptions);
        }
        else if (reader.TokenType == JsonTokenType.StartArray)
        {
            // Array format (for compatibility) - take only the first element
            reader.Read();
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                return null; // Empty array
            }

            if (reader.TokenType == JsonTokenType.StartObject)
            {
                var subject = DeserializeAchievementSubject(ref reader, newOptions);
                
                // Skip any additional array elements
                while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                {
                    reader.Skip();
                }

                return subject;
            }
        }

        throw new JsonException($"Unexpected token type: {reader.TokenType}");
    }

    private AchievementSubject DeserializeAchievementSubject(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        // We MUST manually deserialize to avoid triggering the class-level [JsonConverter] attribute
        // on AchievementSubject which would cause infinite recursion
        
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected StartObject token");
        }

        var instance = new AchievementSubject
        {
            Achievement = null!, // Will be set from JSON
            Type = new System.Collections.ObjectModel.Collection<string>()
        };

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException("Expected PropertyName token");
            }

            var propertyName = reader.GetString();
            
            // Find the matching property
            var property = typeof(AchievementSubject).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(p =>
                {
                    var jsonNameAttr = p.GetCustomAttribute<JsonPropertyNameAttribute>();
                    return jsonNameAttr?.Name == propertyName;
                });

            // Read the property value
            reader.Read();

            if (property != null && property.CanWrite)
            {
                try
                {
                    var value = JsonSerializer.Deserialize(ref reader, property.PropertyType, options);
                    property.SetValue(instance, value);
                }
                catch
                {
                    reader.Skip();
                }
            }
            else
            {
                reader.Skip();
            }
        }

        return instance;
    }

    public override void Write(Utf8JsonWriter writer, AchievementSubject value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        // Create new options WITHOUT AchievementSubjectConverter to prevent infinite recursion
        var newOptions = new JsonSerializerOptions(options);
        newOptions.Converters.Clear();
        
        foreach (var converter in options.Converters)
        {
            if (converter is not AchievementSubjectConverter && converter is not SingleAchievementSubjectConverter)
            {
                newOptions.Converters.Add(converter);
            }
        }

        // Manually serialize to avoid triggering class-level converter
        writer.WriteStartObject();

        var properties = typeof(AchievementSubject).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => !p.GetIndexParameters().Any());

        foreach (var property in properties)
        {
            // Skip [JsonExtensionData] properties
            if (property.GetCustomAttribute<System.Text.Json.Serialization.JsonExtensionDataAttribute>() != null)
                continue;

            var jsonPropertyNameAttr = property.GetCustomAttribute<JsonPropertyNameAttribute>();
            if (jsonPropertyNameAttr == null)
                continue;

            // Use runtime type so derived-class backing fields are read.
            // Use GetProperties().FirstOrDefault() instead of GetProperty() to avoid
            // AmbiguousMatchException when a derived class shadows a base property with 'new'.
            var runtimeProp = value.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(p => p.Name == property.Name);
            object? propertyValue;
            try { propertyValue = (runtimeProp ?? property).GetValue(value); }
            catch { continue; }

            // Check JsonIgnore on the most-derived declaration
            var jsonIgnoreAttr = (runtimeProp ?? property).GetCustomAttribute<JsonIgnoreAttribute>()
                ?? property.GetCustomAttribute<JsonIgnoreAttribute>();
            if (jsonIgnoreAttr?.Condition == JsonIgnoreCondition.Always)
                continue;
            if (jsonIgnoreAttr?.Condition == JsonIgnoreCondition.WhenWritingNull)
            {
                if (propertyValue == null) continue;
                if (propertyValue is System.Collections.ICollection col && col.Count == 0) continue;
                if (propertyValue is System.Collections.IEnumerable en &&
                    propertyValue.GetType() != typeof(string) &&
                    !en.Cast<object>().Any()) continue;
            }

            writer.WritePropertyName(jsonPropertyNameAttr.Name);
            JsonSerializer.Serialize(writer, propertyValue, property.PropertyType, newOptions);
        }

        writer.WriteEndObject();
    }
}
