using Microsoft.Extensions.DependencyInjection;
using EveHelper.Core.Interfaces;

namespace EveHelper.Data.Extensions
{
    /// <summary>
    /// Extension methods for registering Data services
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Data layer dependencies to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection AddData(this IServiceCollection services)
        {
            // Register data services here as they are created
            // Example: services.AddScoped<IDataService, DataService>();
            
            return services;
        }
    }
} 