using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Hr.DAL.Data;
using Hr.DAL.Repositories;
using Hr.DAL.Interfaces.MarkerInterfaces;
using Hr.DAL.Interfaces.RepositoriesInterfaces;

namespace Hr.DAL
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDalDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            // Auto-register using marker interfaces (ITransientService, IScopedService, ISingletonService)
            var assembly = Hr.DAL.AssemblyReference.Assembly;

            services.Scan(scan => scan
                .FromAssemblies(assembly)
                .AddClasses(classes => classes.AssignableTo<Hr.DAL.Interfaces.ITransientService>())
                .AsImplementedInterfaces()
                .WithTransientLifetime());

            services.Scan(scan => scan
                .FromAssemblies(assembly)
                .AddClasses(classes => classes.AssignableTo<Hr.DAL.Interfaces.IScopedService>())
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            services.Scan(scan => scan
                .FromAssemblies(assembly)
                .AddClasses(classes => classes.AssignableTo<Hr.DAL.Interfaces.ISingletonService>())
                .AsImplementedInterfaces()
                .WithSingletonLifetime());

            // Lazy<T> wrappers — consumed by UnitOfWork for lazy initialization.
            // The factory delegate defers resolution until .Value is first accessed.
            services.AddScoped(sp => new Lazy<IEmployeeRepository>(sp.GetRequiredService<IEmployeeRepository>));
            services.AddScoped(sp => new Lazy<ILeaveRepository>(sp.GetRequiredService<ILeaveRepository>));
            services.AddScoped(sp => new Lazy<IAddressRepository>(sp.GetRequiredService<IAddressRepository>));

            return services;
        }
    }
}


