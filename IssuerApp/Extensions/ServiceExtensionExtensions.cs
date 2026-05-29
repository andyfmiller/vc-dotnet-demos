using Library.Crypto;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IssuerApp.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds a <see cref="JsonSerializerOptions"/> singleton for serializing
        /// CLR objects. Null values will be ignored (including empty arrays) and
        /// the output will be indented.
        /// </summary>
        public static IServiceCollection AddJsonSerializerOptions(this IServiceCollection services)
        {
            return services.AddSingleton(new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter() }
            });
        }

        /// <summary>
        /// Registers <see cref="IJsonLdCanonicalizationService"/> pre-seeded with the
        /// well-known context documents bundled in the Library assembly.
        /// </summary>
        public static IServiceCollection AddJsonLdCanonicalizationService(
            this IServiceCollection services)
        {
            return services.AddSingleton<IJsonLdCanonicalizationService>(
                JsonLdCanonicalizationService.CreateWithLibraryContexts());
        }
    }
}
