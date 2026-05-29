using Library.Crypto;

namespace WalletApp.Extensions
{
    public static class ServiceCollectionExtensions
    {
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
