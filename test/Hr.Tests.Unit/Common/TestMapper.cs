using Hr.BLL.Mappings;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;

namespace Hr.Tests.Unit.Common;

/// <summary>
/// Builds a real Mapster IMapper configured with the application's MappingConfig.
/// Used in unit tests so mapping behaviour is tested alongside service logic.
/// </summary>
public static class TestMapper
{
    private static readonly Lazy<IMapper> _instance = new(Build);

    public static IMapper Instance => _instance.Value;

    private static IMapper Build()
    {
        var config = new TypeAdapterConfig();
        new MappingConfig().Register(config);

        var services = new ServiceCollection();
        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();

        return services.BuildServiceProvider().GetRequiredService<IMapper>();
    }
}
