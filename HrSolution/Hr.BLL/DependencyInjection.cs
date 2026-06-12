using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Hr.DAL;

namespace Hr.BLL
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddBllDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            // Register DAL dependencies
            services.AddDalDependencies(configuration);

            // Register BLL services here...

            return services;
        }
    }
}
