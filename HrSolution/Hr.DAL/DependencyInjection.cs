using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Hr.DAL.Data;
using Hr.DAL.Repositories;
using Hr.DAL.Repositories.Interfaces;
using Hr.DAL.UnitOfWork;

namespace Hr.DAL
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDalDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            // Auto-register all repositories and the Unit of Work using Scrutor
            services.Scan(scan => scan
                .FromAssemblyOf<ApplicationDbContext>()

                // Repositories: IFooRepository → FooRepository (Scoped)
                .AddClasses(classes => classes.InNamespaces("Hr.DAL.Repositories"))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()

                // Unit of Work: IUnitOfWork → UnitOfWork (Scoped)
                .AddClasses(classes => classes.InNamespaces("Hr.DAL.UnitOfWork"))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
            );

            // Lazy<T> wrappers — consumed by UnitOfWork for lazy initialization.
            // The factory delegate defers resolution until .Value is first accessed.
            services.AddScoped(sp => new Lazy<IEmployeeRepository>(sp.GetRequiredService<IEmployeeRepository>));
            services.AddScoped(sp => new Lazy<ILeaveRepository>(sp.GetRequiredService<ILeaveRepository>));
            services.AddScoped(sp => new Lazy<IAddressRepository>(sp.GetRequiredService<IAddressRepository>));

            return services;
        }
    }
}


