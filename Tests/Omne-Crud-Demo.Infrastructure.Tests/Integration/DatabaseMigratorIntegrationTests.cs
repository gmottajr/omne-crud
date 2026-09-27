using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using Omne_Crud_Demo.Core.Exceptions;
using Omne_Crud_Demo.DatabaseMigrator;
using Omne_Crud_Demo.Infrastructure.Persistence.Data;

namespace Omne_Crud_Demo.Infrastructure.Tests.Integration;

[Collection(InfrastructureIntegrationCollection.Name)]
public sealed class DatabaseMigratorIntegrationTests
{
    private const string InitialMigration = "20260908093837_InitialCreate";
    private readonly InfrastructureDatabaseFixture _fixture;

    public DatabaseMigratorIntegrationTests(
        InfrastructureDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Migrator_ShouldApplyPendingMigrations_WhenSchemaIsEmpty()
    {
        await RunInIsolatedSchemaAsync(async connectionString =>
        {
            var result = await RunMigratorAsync(connectionString);

            AssertMigratorSucceeded(result);
            Assert.True(await TableExistsAsync(connectionString, "products"));
            Assert.Equal(
                1,
                await GetMigrationCountAsync(
                    connectionString,
                    InitialMigration));
        });
    }

    [Fact]
    public async Task Migrator_ShouldSucceed_WhenNoMigrationsArePending()
    {
        await RunInIsolatedSchemaAsync(async connectionString =>
        {
            var firstRun = await RunMigratorAsync(connectionString);
            var secondRun = await RunMigratorAsync(connectionString);

            AssertMigratorSucceeded(firstRun);
            AssertMigratorSucceeded(secondRun);
            Assert.Equal(
                1,
                await GetMigrationCountAsync(
                    connectionString,
                    InitialMigration));
        });
    }

    [Fact]
    public async Task Migrator_ShouldReturnFailure_WhenDatabaseIsUnavailable()
    {
        var result = await RunMigratorAsync(
            CreateUnavailableConnectionString());

        Assert.Equal(1, result.ExitCode);
        Assert.Contains(
            "Database migration failed.",
            result.CombinedOutput,
            StringComparison.OrdinalIgnoreCase);
        Assert.Contains(
            nameof(DatabaseMigrationExecutionException),
            result.CombinedOutput,
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task Migrator_ShouldReturnFailure_WhenConnectionStringIsMissing()
    {
        var result = await RunMigratorAsync(connectionString: null);

        Assert.Equal(1, result.ExitCode);
        Assert.Contains(
            "Database migration failed.",
            result.CombinedOutput,
            StringComparison.OrdinalIgnoreCase);
        Assert.Contains(
            "Connection string 'DefaultConnection' was not configured.",
            result.CombinedOutput,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Migrator_ShouldRejectConfiguration_WhenNoMigrationsAreFound()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(
                _fixture.ConnectionString,
                npgsql => npgsql.MigrationsAssembly(
                    typeof(DatabaseMigratorIntegrationTests)
                        .Assembly
                        .GetName()
                        .Name))
            .Options;

        await using var context = new AppDbContext(
            options,
            NullLogger<AppDbContext>.Instance);

        var exception = await Assert.ThrowsAsync<NoDatabaseMigrationsFoundException>(
            () => DatabaseMigrationRunner.MigrateAsync(context));

        Assert.Equal(
            "No EF Core migrations were found for AppDbContext.",
            exception.Message);
    }

    [Fact]
    public async Task Runner_ShouldWrapProviderException_WhenMigrationFails()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(CreateUnavailableConnectionString())
            .Options;

        await using var context = new AppDbContext(
            options,
            NullLogger<AppDbContext>.Instance);

        var exception =
            await Assert.ThrowsAsync<DatabaseMigrationExecutionException>(
                () => DatabaseMigrationRunner.MigrateAsync(context));

        Assert.Equal(
            "An error occurred while applying EF Core database migrations.",
            exception.Message);
        Assert.IsAssignableFrom<NpgsqlException>(exception.InnerException);
    }

    [Fact]
    public async Task Runner_ShouldPreserveCancellation_WhenCancellationIsRequested()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(CreateUnavailableConnectionString())
            .Options;

        await using var context = new AppDbContext(
            options,
            NullLogger<AppDbContext>.Instance);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => DatabaseMigrationRunner.MigrateAsync(
                context,
                cancellation.Token));
    }

    [Fact]
    public async Task Runner_ShouldRejectNullDbContext()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => DatabaseMigrationRunner.MigrateAsync(null!));
    }

    private async Task RunInIsolatedSchemaAsync(
        Func<string, Task> test)
    {
        var schemaName = $"migrator_test_{Guid.NewGuid():N}";
        var quotedSchemaName = QuoteIdentifier(schemaName);

        await ExecuteDatabaseCommandAsync(
            $"CREATE SCHEMA {quotedSchemaName};");

        try
        {
            var connectionString =
                new NpgsqlConnectionStringBuilder(_fixture.ConnectionString)
                {
                    SearchPath = schemaName
                }
                .ConnectionString;

            await test(connectionString);
        }
        finally
        {
            await ExecuteDatabaseCommandAsync(
                $"DROP SCHEMA IF EXISTS {quotedSchemaName} CASCADE;");
        }
    }

    private async Task ExecuteDatabaseCommandAsync(string commandText)
    {
        await using var connection =
            new NpgsqlConnection(_fixture.ConnectionString);
        await connection.OpenAsync();

        await using var command =
            new NpgsqlCommand(commandText, connection);
        await command.ExecuteNonQueryAsync();
    }

    private static async Task<bool> TableExistsAsync(
        string connectionString,
        string tableName)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            """
            SELECT EXISTS (
                SELECT 1
                FROM information_schema.tables
                WHERE table_schema = current_schema()
                  AND table_name = @tableName
            );
            """,
            connection);
        command.Parameters.AddWithValue("tableName", tableName);

        return (bool)(await command.ExecuteScalarAsync())!;
    }

    private static async Task<int> GetMigrationCountAsync(
        string connectionString,
        string migrationId)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            """
            SELECT COUNT(*)
            FROM "__EFMigrationsHistory"
            WHERE "MigrationId" = @migrationId;
            """,
            connection);
        command.Parameters.AddWithValue("migrationId", migrationId);

        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    private static void AssertMigratorSucceeded(MigratorResult result)
    {
        Assert.True(
            result.ExitCode == 0,
            $"Migrator failed with exit code {result.ExitCode}. " +
            result.CombinedOutput);
        Assert.Contains(
            "Database migrations applied successfully.",
            result.CombinedOutput,
            StringComparison.OrdinalIgnoreCase);
    }

    private string CreateUnavailableConnectionString()
    {
        return new NpgsqlConnectionStringBuilder(_fixture.ConnectionString)
        {
            Host = "127.0.0.1",
            Port = 1,
            Timeout = 1
        }.ConnectionString;
    }

    private static async Task<MigratorResult> RunMigratorAsync(
        string? connectionString)
    {
        var repositoryRoot = FindRepositoryRoot();
        var testOutputDirectory = new DirectoryInfo(AppContext.BaseDirectory);
        var configuration = testOutputDirectory.Parent?.Name;
        var targetFramework = testOutputDirectory.Name;

        Assert.False(
            string.IsNullOrWhiteSpace(configuration),
            "Could not determine the test build configuration.");

        var migratorOutputDirectory = Path.Combine(
            repositoryRoot,
            "Omne-Crud-Demo.DatabaseMigrator",
            "bin",
            configuration!,
            targetFramework);
        var migratorAssembly = Path.Combine(
            migratorOutputDirectory,
            "Omne-Crud-Demo.DatabaseMigrator.dll");

        Assert.True(
            File.Exists(migratorAssembly),
            $"Migrator assembly was not found at '{migratorAssembly}'.");

        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            WorkingDirectory = migratorOutputDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        startInfo.ArgumentList.Add(migratorAssembly);
        if (connectionString is null)
        {
            startInfo.Environment.Remove(
                "ConnectionStrings__DefaultConnection");
        }
        else
        {
            startInfo.Environment["ConnectionStrings__DefaultConnection"] =
                connectionString;
        }

        using var process = Process.Start(startInfo);

        Assert.NotNull(process);

        var standardOutputTask = process.StandardOutput.ReadToEndAsync();
        var standardErrorTask = process.StandardError.ReadToEndAsync();

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        try
        {
            await process.WaitForExitAsync(timeout.Token);
        }
        catch (OperationCanceledException)
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync();
            }

            var standardOutput = await standardOutputTask;
            var standardError = await standardErrorTask;

            throw new TimeoutException(
                "The database migrator did not exit within 30 seconds. " +
                $"Standard output: {standardOutput} " +
                $"Standard error: {standardError}");
        }

        return new MigratorResult(
            process.ExitCode,
            await standardOutputTask,
            await standardErrorTask);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Omne-Crud-Demo.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            "Could not locate the repository root from the test output directory.");
    }

    private static string QuoteIdentifier(string identifier)
    {
        return $"\"{identifier.Replace("\"", "\"\"")}\"";
    }

    private sealed record MigratorResult(
        int ExitCode,
        string StandardOutput,
        string StandardError)
    {
        public string CombinedOutput => StandardOutput + StandardError;
    }
}
