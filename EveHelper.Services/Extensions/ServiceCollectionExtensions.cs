using System;
using Microsoft.Extensions.DependencyInjection;
using EveHelper.Core.Interfaces;
using EveHelper.Core.Models.Authentication;
using EveHelper.Core.Models.Configuration;
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
            // Configure EVE authentication with default settings
            services.AddSingleton(new EveAuthConfiguration
            {
                ClientId = "3fe0363633d34abcb6bc0d50d9d2c9f8",
                ClientSecret = "Sc5A6JfqljzPiYczdNDcJ5HETqeo01ORzgXHaELQ",
                CallbackUrl = "http://localhost:5000/callback",
                EsiBaseUrl = "https://esi.evetech.net"
            });
            
            // Configure token storage with default settings
            services.AddSingleton(new TokenStorageConfiguration
            {
                UseCredentialManager = true,
                UseFallbackFileStorage = true,
                RefreshThreshold = TimeSpan.FromMinutes(5),
                MonitoringInterval = TimeSpan.FromMinutes(1),
                AutoStartMonitoring = true,
                EnableLogging = true
            });
            
            // Configure ESI client with default settings
            services.AddSingleton(new EsiClientConfiguration
            {
                BaseUrl = "https://esi.evetech.net",
                Datasource = "tranquility",
                TimeoutSeconds = 30,
                MaxRetryAttempts = 3,
                RetryDelay = TimeSpan.FromMilliseconds(500),
                UseExponentialBackoff = true,
                RespectCacheHeaders = true,
                RespectRateLimiting = true,
                UserAgent = "EveHelper/1.0 (https://github.com/korallis/EveHelper)",
                EnableLogging = true,
                LogRequestBodies = false,
                MaxLogContentLength = 10240
            });
            
            // Register HTTP client for EVE authentication
            services.AddHttpClient<IEveAuthService, EveAuthService>();
            
            // Register HTTP client for ESI API calls
            services.AddHttpClient<IEsiClientService, EsiClientService>();
            
            // Register the authentication service
            services.AddScoped<IEveAuthService, EveAuthService>();
            
            // Register the token storage service
            services.AddScoped<ITokenStorageService, TokenStorageService>();
            
            // Register the ESI client service
            services.AddScoped<IEsiClientService, EsiClientService>();
            
            return services;
        }
    }
} 