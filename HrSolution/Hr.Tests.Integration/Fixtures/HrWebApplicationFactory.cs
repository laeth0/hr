using Hr.DAL.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Hr.Tests.Integration.Fixtures;

/// <summary>
/// Boots the full ASP.NET Core pipeline with an isolated EF Core InMemory database.
/// Each factory instance gets its own unique DB name so test classes never share state.
/// </summary>
public sealed class HrWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly string _dbName = $"HrTestDb_{Guid.NewGuid():N}";

    // Shared root keeps all DbContext instances (seed scopes + request scopes) pointing
    // at the same in-memory store.  Required when EF Core's internal service provider
    // is shared across instances (the default, with caching enabled).
    private readonly InMemoryDatabaseRoot _dbRoot = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // ConfigureTestServices runs AFTER the app's own DI registrations, so we can
        // safely replace whatever AddDalDependencies registered.
        builder.ConfigureTestServices(services =>
        {
            // EF Core 10's AddDbContext registers TWO entries:
            //   1. IDbContextOptionsConfiguration<T>  – the Npgsql configuration action
            //   2. DbContextOptions<T>                – built lazily from all configurators
            //
            // Removing only DbContextOptions<T> leaves the Npgsql configurator in place,
            // so when InMemory's configurator is added both providers end up in the same
            // options and EF Core throws.  We must remove the configurators first.
            var configuratorType = typeof(ApplicationDbContext).Assembly
                .GetType("Microsoft.EntityFrameworkCore.Infrastructure.IDbContextOptionsConfiguration`1")
                ?.MakeGenericType(typeof(ApplicationDbContext));

            if (configuratorType is not null)
            {
                var toRemove = services
                    .Where(d => d.ServiceType == configuratorType)
                    .ToList();
                foreach (var d in toRemove)
                    services.Remove(d);
            }
            else
            {
                // Fallback: match by name for forward-compatibility
                var toRemove = services
                    .Where(d => d.ServiceType.IsGenericType &&
                                d.ServiceType.Name.StartsWith("IDbContextOptionsConfiguration",
                                    StringComparison.Ordinal))
                    .ToList();
                foreach (var d in toRemove)
                    services.Remove(d);
            }

            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<ApplicationDbContext>();

            // Fresh InMemory DB isolated to this factory instance.
            // Passing _dbRoot ensures seed scopes and request handler scopes share
            // the same in-memory store (EF Core's internal service provider caches
            // the InMemoryDatabaseRoot as a singleton keyed on this object).
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(_dbName, _dbRoot));
        });
    }

    public async Task InitializeAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.EnsureCreatedAsync();
    }

    public new Task DisposeAsync() => base.DisposeAsync().AsTask();

    /// <summary>
    /// Runs <paramref name="seed"/> inside a fresh DI scope so tests can populate
    /// the database before issuing HTTP requests.
    /// </summary>
    public async Task SeedAsync(Func<ApplicationDbContext, Task> seed)
    {
        await using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await seed(db);
        await db.SaveChangesAsync();
    }

    /// <summary>Removes all rows from all tables to isolate individual tests.</summary>
    public async Task ResetDatabaseAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Leaves.RemoveRange(db.Leaves);
        db.Addresses.RemoveRange(db.Addresses);
        db.Employees.RemoveRange(db.Employees);
        await db.SaveChangesAsync();
    }
}
