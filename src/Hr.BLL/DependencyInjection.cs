using FluentValidation;
using Hr.DAL;
using Hr.DAL.Interfaces.MarkerInterfaces;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Hr.BLL
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddBllDependencies(
            this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDalDependencies(configuration);

            var assembly = Assembly.GetExecutingAssembly();

            // Mapster — scan assembly for IRegister implementations (MappingConfig)
            var typeAdapterConfig = TypeAdapterConfig.GlobalSettings;
            typeAdapterConfig.Scan(assembly);
            services.AddSingleton(typeAdapterConfig);
            services.AddScoped<IMapper, ServiceMapper>();

            // FluentValidation — auto-register all validators in this assembly
            services.AddValidatorsFromAssembly(assembly);

            // Auto-register BLL services that implement IScopedService
            services.Scan(scan => scan
                .FromAssemblies(assembly)
                .AddClasses(classes => classes.AssignableTo<IScopedService>())
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            return services;
        }
    }
}
