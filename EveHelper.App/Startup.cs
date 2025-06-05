using Microsoft.Extensions.DependencyInjection;
using EveHelper.Core.Extensions;
using EveHelper.Data.Extensions;
using EveHelper.Services.Extensions;
using EveHelper.App.Extensions;
using System;

namespace EveHelper.App
{
    /// <summary>
    /// Handles application startup configuration and dependency injection setup
    /// </summary>
    public static class Startup
    {
        /// <summary>
        /// Configures and builds the service provider for the application
        /// </summary>
        /// <returns>Configured service provider</returns>
        public static ServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // Register services from each layer
            services.AddCore();           // Core interfaces and models
            services.AddData();           // Data access layer
            services.AddServices();       // Business services layer  
            services.AddApp();            // Application layer (ViewModels, Views)

            return services.BuildServiceProvider();
        }
    }
} 