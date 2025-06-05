using Microsoft.Extensions.DependencyInjection;
using EveHelper.App.ViewModels;
using EveHelper.App.Views;
using EveHelper.App.Services;
using EveHelper.Core.Interfaces;

namespace EveHelper.App.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds App layer dependencies to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection AddApp(this IServiceCollection services)
        {
            // Register Services
            services.AddSingleton<INavigationService, NavigationService>();
            
            // Register ViewModels
            services.AddTransient<HomeViewModel>();
            
            // Register Views
            services.AddTransient<HomeView>();
            
            return services;
        }
    }
} 