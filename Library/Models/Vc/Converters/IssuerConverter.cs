namespace Library.Models.Vc.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Converts an Issuer to and from JSON. Handles both string URL form and object form.
    /// </summary>
    public class IssuerConverter : JsonConverter<Issuer>
    {
        public override Issuer? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                // Simple URL form: "issuer": "https://example.com/issuer"
                var url = reader.GetString();
                return url != null ? new Issuer { Id = url } : null;
            }
            else if (reader.TokenType == JsonTokenType.StartObject)
            {
                // Object form: "issuer": { "id": "https://example.com/issuer", ... }
                using var doc = JsonDocument.ParseValue(ref reader);
                var root = doc.RootElement;

                if (!root.TryGetProperty("id", out var idElement))
                {
                    throw new JsonException("Issuer object must contain an 'id' property.");
                }

                var issuer = new Issuer { Id = idElement.GetString()! };

                // Read additional properties
                var additionalProps = new Dictionary<string, object>();
                foreach (var property in root.EnumerateObject())
                {
                    if (property.Name != "id")
                    {
                        additionalProps[property.Name] = JsonSerializer.Deserialize<object>(property.Value.GetRawText(), options)!;
                    }
                }

                if (additionalProps.Count > 0)
                {
                    issuer.AdditionalProperties = additionalProps;
                }

                return issuer;
            }

            throw new JsonException("Issuer must be either a string URL or an object with an 'id' property.");
        }

        public override void Write(Utf8JsonWriter writer, Issuer value, JsonSerializerOptions options)
        {
            if (value.AdditionalProperties == null || value.AdditionalProperties.Count == 0)
            {
                // Simple URL form - write as string
                writer.WriteStringValue(value.Id);
            }
            else
            {
                // Object form - write as object with id and additional properties
                writer.WriteStartObject();
                writer.WriteString("id", value.Id);

                foreach (var prop in value.AdditionalProperties)
                {
                    writer.WritePropertyName(prop.Key);
                    JsonSerializer.Serialize(writer, prop.Value, options);
                }

                writer.WriteEndObject();
            }
        }
    }
}