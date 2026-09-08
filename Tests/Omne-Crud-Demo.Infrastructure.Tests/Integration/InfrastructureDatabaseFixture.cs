using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using Omne_Crud_Demo.Domain;
using Omne_Crud_Demo.Infrastructure.Persistence.Data;

namespace Omne_Crud_Demo.Infrastructure.Tests.Integration;

public sealed class InfrastructureDatabaseFixture : IAsyncLifetime
{
    private DbContextOptions<AppDbContext>? _options;

    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
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

        ConnectionString =
            configuration.GetConnectionString("TestConnectionString")
            ?? throw new InvalidOperationException(
                "Connection string 'TestConnectionString' was not found.");

        ValidateTestDatabase(ConnectionString);

        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(
                ConnectionString,
                options =>
                    options.MigrationsAssembly(
                        typeof(AppDbContext).Assembly.GetName().Name))
            .EnableDetailedErrors()
            .Options;

        await using var context = CreateDbContext();

        await context.Database.MigrateAsync();
    }

    public AppDbContext CreateDbContext()
    {
        if (_options is null)
        {
            throw new InvalidOperationException(
                "Database fixture has not been initialized.");
        }

        return new AppDbContext(
            _options,
            NullLogger<AppDbContext>.Instance);
    }

    public async Task ResetDatabaseAsync()
    {
        await using var context = CreateDbContext();

        await context
            .Set<Product>()
            .ExecuteDeleteAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    private static void ValidateTestDatabase(string connectionString)
    {
        var builder =
            new NpgsqlConnectionStringBuilder(connectionString);

        if (string.IsNullOrWhiteSpace(builder.Database) ||
            !builder.Database.Contains(
                "test",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Refusing to run integration tests against database '{builder.Database}'. " +
                "The test database name must contain 'test'.");
        }
    }
}
