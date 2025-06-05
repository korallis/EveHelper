using EveHelper.Core.Interfaces;
using EveHelper.Services.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EveHelper.Services
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddSingleton<ITokenStorageService, TokenStorageService>();
            services.AddSingleton<IEsiClientService, EsiClientService>();
            services.AddSingleton<IEveAuthService, EveAuthService>();
            services.AddSingleton<ICharacterService, CharacterService>();
            services.AddSingleton<ICharacterDataService, CharacterDataService>();

            return services;
        }
    }
} 