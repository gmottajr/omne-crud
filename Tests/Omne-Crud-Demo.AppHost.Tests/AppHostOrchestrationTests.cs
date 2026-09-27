using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.Logging.Abstractions;

namespace Omne_Crud_Demo.AppHost.Tests;

public sealed class AppHostOrchestrationTests
{
    private const string DefaultConnectionEnvironmentVariable =
        "ConnectionStrings__DefaultConnection";

    [Fact]
    public async Task AppHost_ShouldRegisterTheCompleteApplicationTopology()
    {
        await using var builder = await CreateAppHostBuilderAsync();

        var resourcesByName = builder.Resources.ToDictionary(resource => resource.Name);

        Assert.IsType<PostgresServerResource>(resourcesByName["postgres"]);
        Assert.IsType<PostgresDatabaseResource>(resourcesByName["productsdb"]);
        Assert.IsType<ProjectResource>(resourcesByName["database-migrator"]);
        Assert.IsType<ProjectResource>(resourcesByName["server"]);
        Assert.True(resourcesByName.ContainsKey("webfrontend"));
    }

    [Fact]
    public async Task DatabaseMigrator_ShouldReceiveDatabaseConnectionAndWaitForDatabase()
    {
        await using var builder = await CreateAppHostBuilderAsync();

        var productsDatabase = GetResource<PostgresDatabaseResource>(builder, "productsdb");
        var databaseMigrator = GetResource<ProjectResource>(builder, "database-migrator");

        Assert.Equal(
            "Omne-Crud-Demo.DatabaseMigrator.csproj",
            Path.GetFileName(databaseMigrator.GetProjectMetadata().ProjectPath));

        var environmentVariables = await GetEnvironmentVariablesAsync(databaseMigrator);

        Assert.Contains(DefaultConnectionEnvironmentVariable, environmentVariables.Keys);
        Assert.Contains(
            databaseMigrator.Annotations.OfType<ResourceRelationshipAnnotation>(),
            annotation => ReferenceEquals(annotation.Resource, productsDatabase));

        var databaseWait = Assert.Single(
            databaseMigrator.Annotations.OfType<WaitAnnotation>(),
            annotation => ReferenceEquals(annotation.Resource, productsDatabase));

        Assert.Equal(WaitType.WaitUntilHealthy, databaseWait.WaitType);
    }

    [Fact]
    public async Task Server_ShouldReceiveDatabaseConnectionAndWaitForSuccessfulMigratorCompletion()
    {
        await using var builder = await CreateAppHostBuilderAsync();

        var productsDatabase = GetResource<PostgresDatabaseResource>(builder, "productsdb");
        var databaseMigrator = GetResource<ProjectResource>(builder, "database-migrator");
        var server = GetResource<ProjectResource>(builder, "server");

        var environmentVariables = await GetEnvironmentVariablesAsync(server);

        Assert.Contains(DefaultConnectionEnvironmentVariable, environmentVariables.Keys);
        Assert.Contains(
            server.Annotations.OfType<ResourceRelationshipAnnotation>(),
            annotation => ReferenceEquals(annotation.Resource, productsDatabase));

        var migratorWait = Assert.Single(
            server.Annotations.OfType<WaitAnnotation>(),
            annotation => ReferenceEquals(annotation.Resource, databaseMigrator));

        Assert.Equal(WaitType.WaitForCompletion, migratorWait.WaitType);
        Assert.Equal(0, migratorWait.ExitCode);
    }

    [Fact]
    public async Task WebFrontend_ShouldReferenceAndWaitForHealthyServer()
    {
        await using var builder = await CreateAppHostBuilderAsync();

        var server = GetResource<ProjectResource>(builder, "server");
        var webFrontend = Assert.Single(
            builder.Resources,
            resource => resource.Name == "webfrontend");

        Assert.Single(server.Annotations.OfType<HealthCheckAnnotation>());
        Assert.Contains(
            webFrontend.Annotations.OfType<ResourceRelationshipAnnotation>(),
            annotation => ReferenceEquals(annotation.Resource, server));

        var serverWait = Assert.Single(
            webFrontend.Annotations.OfType<WaitAnnotation>(),
            annotation => ReferenceEquals(annotation.Resource, server));

        Assert.Equal(WaitType.WaitUntilHealthy, serverWait.WaitType);
    }

    private static Task<IDistributedApplicationTestingBuilder> CreateAppHostBuilderAsync()
    {
        return DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Omne_Crud_Demo_AppHost>();
    }

    private static async Task<IReadOnlyDictionary<string, string>> GetEnvironmentVariablesAsync(
        IResource resource)
    {
        var executionContext = new DistributedApplicationExecutionContext(
            DistributedApplicationOperation.Publish);

        var executionConfiguration = await ExecutionConfigurationBuilder
            .Create(resource)
            .WithEnvironmentVariablesConfig()
            .BuildAsync(
                executionContext,
                NullLogger.Instance,
                CancellationToken.None);

        return executionConfiguration.EnvironmentVariables.ToDictionary(
            variable => variable.Key,
            variable => variable.Value);
    }

    private static TResource GetResource<TResource>(
        IDistributedApplicationTestingBuilder builder,
        string resourceName)
        where TResource : class, IResource
    {
        return Assert.IsType<TResource>(
            Assert.Single(builder.Resources, resource => resource.Name == resourceName));
    }
}
