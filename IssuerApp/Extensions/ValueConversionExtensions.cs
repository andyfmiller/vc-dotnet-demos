using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IssuerApp.Extensions
{
    public static class ValueConversionExtensions
    {
        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static PropertyBuilder<T> HasJsonConversion<T>(this PropertyBuilder<T> propertyBuilder)
        {
            var converter = new ValueConverter<T, string>(
                v => JsonSerializer.Serialize(v, JsonSerializerOptions),
                v => JsonSerializer.Deserialize<T>(v, JsonSerializerOptions)!);

            var comparer = new ValueComparer<T>(
                (l, r) => JsonSerializer.Serialize(l, JsonSerializerOptions) == JsonSerializer.Serialize(r, JsonSerializerOptions),
                v => v == null ? 0 : JsonSerializer.Serialize(v, JsonSerializerOptions).GetHashCode(),
                v => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(v, JsonSerializerOptions), JsonSerializerOptions)!);

            propertyBuilder.HasConversion(converter);
            propertyBuilder.Metadata.SetValueConverter(converter);
            propertyBuilder.Metadata.SetValueComparer(comparer);

            return propertyBuilder;
        }
    }
}
