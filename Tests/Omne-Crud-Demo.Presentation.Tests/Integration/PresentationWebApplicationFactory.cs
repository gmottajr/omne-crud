using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Npgsql;
using Omne_Crud_Demo.Infrastructure.Persistence.Data;

namespace Omne_Crud_Demo.Presentation.Tests.Integration;

public sealed class PresentationWebApplicationFactory
    : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly string _connectionString;

    public PresentationWebApplicationFactory()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile(
                "appsettings.json",
                optional: true,
                reloadOnChange: false)
            .AddUserSecrets(
                typeof(Program).Assembly,
                optional: false)
            .AddEnvironmentVariables()
            .Build();

        _connectionString =
            configuration.GetConnectionString("TestConnectionString")
            ?? throw new InvalidOperationException(
                "Connection string 'TestConnectionString' was not found.");

        ValidateTestDatabase(_connectionString);
    }

    public string ConnectionString => _connectionString;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(
                    ConnectionString,
                    npgsql =>
                        npgsql.MigrationsAssembly(
                            typeof(AppDbContext)
                                .Assembly
                                .GetName()
                                .Name));
            });
        });
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();

        var context =
            scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

        await context.Database.MigrateAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();

        var context =
            scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

        await context.Products.ExecuteDeleteAsync();
    }

    public new Task DisposeAsync()
    {
        Dispose();
        return Task.CompletedTask;
    }

    private static void ValidateTestDatabase(string connectionString)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);

        if (string.IsNullOrWhiteSpace(builder.Database) ||
            !builder.Database.Contains("test", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Refusing to run integration tests against database '{builder.Database}'. " +
                "The test database name must contain 'test'.");
        }
    }
}
