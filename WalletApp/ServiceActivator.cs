using Microsoft.Extensions.DependencyInjection;
using System;

namespace WalletApp
{
    /// <summary>
    /// Add static service resolver to use when dependencies injection is not available
    /// </summary>
    public class ServiceActivator
    {
        internal static IServiceProvider? ServiceProvider;

        /// <summary>
        /// Configure ServiceActivator with full serviceProvider
        /// </summary>
        public static void Configure(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        /// <summary>
        /// Create a scope where use this ServiceActivator
        /// </summary>
        public static IServiceScope? GetScope(IServiceProvider? serviceProvider = null)
        {
            var provider = serviceProvider ?? ServiceProvider;
            return provider?
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope();
        }
    }
}
