using Microsoft.Extensions.DependencyInjection;
using EveHelper.Core.Interfaces;

namespace EveHelper.Core.Extensions
{
    /// <summary>
    /// Extension methods for registering Core services
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Core layer dependencies to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection AddCore(this IServiceCollection services)
        {
            // Register core services and interfaces here as they are created
            // Example: services.AddScoped<ICoreService, CoreService>();
            
            return services;
        }
    }
} 