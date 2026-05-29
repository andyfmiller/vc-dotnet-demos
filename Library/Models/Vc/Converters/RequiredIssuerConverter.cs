namespace Library.Models.Vc.Converters;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// JSON converter factory for the Issuer property that ensures it's not null during deserialization.
/// Supports Issuer and any derived types.
/// </summary>
public class RequiredIssuerConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        // Can convert Issuer or any type that derives from Issuer
        return typeof(Issuer).IsAssignableFrom(typeToConvert);
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        // Create a generic converter for the specific type
        var converterType = typeof(RequiredIssuerConverterInner<>).MakeGenericType(typeToConvert);
        return (JsonConverter?)Activator.CreateInstance(converterType);
    }

    private class RequiredIssuerConverterInner<T> : JsonConverter<T> where T : Issuer
    {
        // This tells System.Text.Json to call our Read method even when the JSON value is null
        public override bool HandleNull => true;

        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                throw new JsonException("The 'issuer' property is required and cannot be null.");
            }

            // Deserialize without the RequiredIssuerConverter to avoid infinite recursion
            var optionsWithoutThis = new JsonSerializerOptions(options);
            optionsWithoutThis.Converters.Clear();
            foreach (var converter in options.Converters)
            {
                if (converter is not RequiredIssuerConverter)
                {
                    optionsWithoutThis.Converters.Add(converter);
                }
            }

            return JsonSerializer.Deserialize<T>(ref reader, optionsWithoutThis);
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                throw new JsonException("The 'issuer' property is required and cannot be null.");
            }

            // Serialize without the RequiredIssuerConverter to avoid infinite recursion
            var optionsWithoutThis = new JsonSerializerOptions(options);
            optionsWithoutThis.Converters.Clear();
            foreach (var converter in options.Converters)
            {
                if (converter is not RequiredIssuerConverter)
                {
                    optionsWithoutThis.Converters.Add(converter);
                }
            }

            JsonSerializer.Serialize(writer, value, optionsWithoutThis);
        }
    }
}