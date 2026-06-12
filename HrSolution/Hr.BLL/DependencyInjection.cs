using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using FluentValidation;
using Mapster;
using MapsterMapper;
using Hr.DAL;

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
