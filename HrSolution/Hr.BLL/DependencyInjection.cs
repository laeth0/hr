using FluentValidation;
using Hr.DAL;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Hr.BLL
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddBllDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            // Register DAL dependencies
            services.AddDalDependencies(configuration);

            var assembly = Assembly.GetExecutingAssembly();

            // Register Mapster mapping configuration
            var typeAdapterConfig = TypeAdapterConfig.GlobalSettings;
            typeAdapterConfig.Scan(assembly);
            services.AddSingleton(typeAdapterConfig);
            services.AddScoped<IMapper, ServiceMapper>();

            // Register FluentValidation validators
            services.AddValidatorsFromAssembly(assembly);

            return services;
        }
    }
}
