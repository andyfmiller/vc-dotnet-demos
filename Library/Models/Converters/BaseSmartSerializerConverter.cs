namespace Library.Models.Converters;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Base class for class-level JSON converters that intelligently handle empty collections.
/// When a property has [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] and is an empty collection,
/// the property is omitted from JSON output while still allowing collections to be initialized in memory.
/// This provides backward compatibility (no NullReferenceExceptions) with clean JSON output.
/// </summary>
/// <typeparam name="T">The type to convert</typeparam>
public abstract class BaseSmartSerializerConverter<T> : JsonConverter<T> where T : class
{
    public override bool CanConvert(Type typeToConvert) => typeof(T).IsAssignableFrom(typeToConvert);

    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // We CANNOT use JsonSerializer.Deserialize<T> because the class-level [JsonConverter] attribute
        // will be respected regardless of the options, causing infinite recursion.
        // Instead, we must manually deserialize each property to completely bypass the converter system.
        
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected StartObject token");
        }

        // Create new options WITHOUT the converter for type T to prevent infinite recursion
        // But keep other BaseSmartSerializerConverter types for nested objects
        var newOptions = new JsonSerializerOptions(options);
        newOptions.Converters.Clear();
        
        foreach (var converter in options.Converters)
        {
            // Skip only the converter for the current type T to prevent infinite recursion
            // Keep all other converters including other BaseSmartSerializerConverter types
            if (converter.GetType() != this.GetType())
            {
                newOptions.Converters.Add(converter);
            }
        }

        // Create an instance of T using the default constructor
        var instance = Activator.CreateInstance(typeToConvert);
        if (instance == null)
        {
            return null;
        }

        // Read the JSON object property by property
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
            
            // Find the matching property on the type
            var property = typeToConvert.GetProperties(BindingFlags.Public | BindingFlags.Instance)
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
                    // Check if property has a custom converter attribute
                    var converterAttr = property.GetCustomAttribute<JsonConverterAttribute>();
                    
                    if (converterAttr != null && converterAttr.ConverterType != null)
                    {
                        // Create options with the property's custom converter added
                        var converterOptions = new JsonSerializerOptions(newOptions);
                        var converter = Activator.CreateInstance(converterAttr.ConverterType) as JsonConverter;
                        if (converter != null)
                        {
                            converterOptions.Converters.Add(converter);
                            var value = JsonSerializer.Deserialize(ref reader, property.PropertyType, converterOptions);
                            property.SetValue(instance, value);
                            continue;
                        }
                    }
                    
                    // Default: Deserialize the property value using the modified options
                    var defaultValue = JsonSerializer.Deserialize(ref reader, property.PropertyType, newOptions);
                    property.SetValue(instance, defaultValue);
                }
                catch
                {
                    // Skip properties that fail to deserialize
                    reader.Skip();
                }
            }
            else
            {
                // Skip unknown properties
                reader.Skip();
            }
        }

        return (T)instance;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        var objectType = typeof(T);
        var properties = objectType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => !p.GetIndexParameters().Any())
            .OrderBy(p => p.GetCustomAttribute<JsonPropertyOrderAttribute>()?.Order ?? 0);

        var seenJsonNames = new HashSet<string>();

        foreach (var property in properties)
        {
            // Skip [JsonExtensionData] properties — they are not named JSON properties
            if (property.GetCustomAttribute<JsonExtensionDataAttribute>() != null)
                continue;

            // Get JSON property name
            var jsonPropertyNameAttr = property.GetCustomAttribute<JsonPropertyNameAttribute>();
            if (jsonPropertyNameAttr == null)
                continue; // Skip properties without JsonPropertyName

            // Get property value using the runtime type so derived-class backing fields are read.
            // Use GetProperties().FirstOrDefault() instead of GetProperty() to avoid
            // AmbiguousMatchException when a derived class shadows a base property with 'new'.
            var runtimeProp = value!.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(p => p.Name == property.Name);

            object? propertyValue;
            try
            {
                propertyValue = (runtimeProp ?? property).GetValue(value);
            }
            catch
            {
                // Skip properties that throw exceptions when accessed
                continue;
            }

            var propertyName = jsonPropertyNameAttr.Name;

            // Deduplicate: when a derived class shadows a base property with 'new', GetProperties()
            // returns both; emit only the first (most-derived) occurrence.
            if (!seenJsonNames.Add(propertyName))
                continue;

            // Check JsonIgnore attribute on the most-derived declaration
            var jsonIgnoreAttr = (runtimeProp ?? property).GetCustomAttribute<JsonIgnoreAttribute>()
                ?? property.GetCustomAttribute<JsonIgnoreAttribute>();
            
            // Determine if we should skip this property
            if (ShouldSkipProperty(propertyValue, jsonIgnoreAttr))
                continue;

            // Write the property name
            writer.WritePropertyName(propertyName);

            // Serialize the property value
            SerializePropertyValue(writer, property, propertyValue, options);
        }

        writer.WriteEndObject();
    }

    /// <summary>
    /// Determines if a property should be skipped during serialization
    /// </summary>
    protected virtual bool ShouldSkipProperty(object? propertyValue, JsonIgnoreAttribute? jsonIgnoreAttr)
    {
        if (jsonIgnoreAttr?.Condition == JsonIgnoreCondition.Always)
            return true;

        if (jsonIgnoreAttr?.Condition == JsonIgnoreCondition.WhenWritingNull)
        {
            if (propertyValue == null)
                return true;

            // SMART LOGIC: Also skip empty collections
            if (propertyValue is ICollection collection && collection.Count == 0)
                return true;

            if (propertyValue is IEnumerable enumerable &&
                propertyValue.GetType() != typeof(string) &&
                !enumerable.Cast<object>().Any())
                return true;
        }

        if (jsonIgnoreAttr?.Condition == JsonIgnoreCondition.WhenWritingDefault)
        {
            if (propertyValue == null)
                return true;

            // Check if it's the default value for the type
            var propertyType = propertyValue.GetType();
            if (propertyType.IsValueType)
            {
                var defaultValue = Activator.CreateInstance(propertyType);
                if (Equals(propertyValue, defaultValue))
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Serializes a property value, using its custom converter if available
    /// </summary>
    protected virtual void SerializePropertyValue(Utf8JsonWriter writer, PropertyInfo property, object? propertyValue, JsonSerializerOptions options)
    {
        // Create new options WITHOUT the current smart serializer to avoid recursion
        var newOptions = new JsonSerializerOptions(options);
        newOptions.Converters.Clear();
        
        foreach (var converter in options.Converters)
        {
            // Skip the current smart serializer converter to prevent recursion
            if (converter.GetType() != this.GetType())
            {
                newOptions.Converters.Add(converter);
            }
        }

        // Check if the property has a custom converter
        var converterAttr = property.GetCustomAttribute<JsonConverterAttribute>();
        if (converterAttr != null && converterAttr.ConverterType != null)
        {
            try
            {
                var converter = Activator.CreateInstance(converterAttr.ConverterType) as JsonConverter;
                if (converter != null)
                {
                    // Use reflection to call the converter's Write method
                    var writeMethod = converterAttr.ConverterType.GetMethod("Write", 
                        BindingFlags.Public | BindingFlags.Instance);
                    
                    if (writeMethod != null)
                    {
                        writeMethod.Invoke(converter, new object[] { writer, propertyValue!, newOptions });
                        return;
                    }
                }
            }
            catch
            {
                // If custom converter fails, fall back to default serialization
            }
        }

        // Default serialization
        JsonSerializer.Serialize(writer, propertyValue, property.PropertyType, newOptions);
    }

    /// <summary>
    /// Checks if a converter is a BaseSmartSerializerConverter (used to prevent recursion)
    /// </summary>
    private static bool IsSmartSerializerConverter(JsonConverter converter)
    {
        var converterType = converter.GetType();
        
        // Check if the converter type inherits from BaseSmartSerializerConverter<>
        var currentType = converterType;
        while (currentType != null && currentType != typeof(object))
        {
            if (currentType.IsGenericType && 
                currentType.GetGenericTypeDefinition() == typeof(BaseSmartSerializerConverter<>))
            {
                return true;
            }
            currentType = currentType.BaseType;
        }
        
        return false;
    }
}
