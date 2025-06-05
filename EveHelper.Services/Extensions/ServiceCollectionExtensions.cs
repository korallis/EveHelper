using Microsoft.Extensions.DependencyInjection;
using EveHelper.Core.Interfaces;
using EveHelper.Core.Models.Authentication;
using EveHelper.Services.Services;

namespace EveHelper.Services.Extensions
{
    /// <summary>
    /// Extension methods for registering Services
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Services layer dependencies to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            // Register EVE authentication configuration
            services.AddSingleton<EveAuthConfiguration>(EveAuthConfiguration.Default);
            
            // Register HTTP client for EVE authentication
            services.AddHttpClient<IEveAuthService, EveAuthService>();
            
            return services;
        }
    }
} 