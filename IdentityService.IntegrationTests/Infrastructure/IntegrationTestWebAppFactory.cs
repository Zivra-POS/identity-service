using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Respawn;
using System.Data.Common;
using Npgsql;
using IdentityService.Infrastructure.Persistence;
using Xunit;
using System.Threading;

namespace IdentityService.IntegrationTests.Infrastructure;

public class IntegrationTestWebAppFactory : WebApplicationFactory<IdentityService.API.Program>, IAsyncLifetime
{
    private DbConnection _connection = null!;
    private Respawner _respawner = null!;
    private readonly string _connectionString;

    // serialize reset operations across multiple test fixtures
    private static readonly SemaphoreSlim _resetSemaphore = new(1, 1);

    public IntegrationTestWebAppFactory()
    {
        _connectionString = GetConnectionString();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(configurationBuilder =>
        {
            var integrationConfig = new ConfigurationBuilder()
                .AddJsonFile("appsettings.Test.json")
                .Build();

            configurationBuilder.AddConfiguration(integrationConfig);
        });

        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<IdentityDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Add test database context
            services.AddDbContext<IdentityDbContext>(options =>
            {
                options.UseNpgsql(_connectionString);
            });

            // NOTE: Do NOT build a service provider and apply migrations here. Building
            // the provider during IServiceCollection configuration and running migrations
            // can cause deadlocks during test host startup. Migrations are applied
            // in InitializeAsync where the host is ready.

            builder.UseEnvironment("Test");
        });
    }

    public async Task InitializeAsync()
    {
        _connection = new NpgsqlConnection(_connectionString);
        await _connection.OpenAsync();

        // Ensure database has tables before creating Respawner
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        
        // Apply migrations to create tables
        await context.Database.MigrateAsync();

        _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions
        {
            TablesToIgnore = new Respawn.Graph.Table[] { "__EFMigrationsHistory" },
            DbAdapter = DbAdapter.Postgres
        });
    }

    public async Task ResetDatabaseAsync()
    {
        await _resetSemaphore.WaitAsync();
        try
        {
            const int maxAttempts = 5;
            int attempt = 0;
            while (true)
            {
                attempt++;
                try
                {
                    await _respawner.ResetAsync(_connection);
                    return;
                }
                catch (Npgsql.PostgresException pgEx) when (pgEx.SqlState == "40P01") // deadlock detected
                {
                    if (attempt >= maxAttempts) throw;
                    // small exponential backoff
                    await Task.Delay(100 * attempt);
                }
                catch (Exception)
                {
                    if (attempt >= maxAttempts) throw;
                    await Task.Delay(100 * attempt);
                }
            }
        }
        finally
        {
            _resetSemaphore.Release();
        }
    }

    public new async Task DisposeAsync()
    {
        await _connection.CloseAsync();
        await _connection.DisposeAsync();
        await base.DisposeAsync();
    }

    private static string GetConnectionString()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Test.json")
            .Build();

        return configuration.GetConnectionString("DefaultConnection")!;
    }
}
