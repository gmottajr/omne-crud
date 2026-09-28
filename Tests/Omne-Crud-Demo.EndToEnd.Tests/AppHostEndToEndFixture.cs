extern alias server;

using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Omne_Crud_Demo.EndToEnd.Tests;

public sealed class AppHostEndToEndFixture : IAsyncLifetime
{
    private static readonly TimeSpan StartupTimeout = TimeSpan.FromMinutes(3);
    private IDistributedApplicationTestingBuilder? _builder;
    private DistributedApplication? _application;

    public Uri WebFrontendEndpoint { get; private set; } = null!;

    public HttpClient CreateServerClient()
    {
        return _application?.CreateHttpClient("server")
            ?? throw new InvalidOperationException(
                "The Aspire application has not been started.");
    }

    public async Task InitializeAsync()
    {
        using var timeout = new CancellationTokenSource(StartupTimeout);

        var testConnectionString = LoadTestConnectionString();

        _builder = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Omne_Crud_Demo_AppHost>(timeout.Token);
        _builder.Configuration["ConnectionStrings:productsdb"] =
            testConnectionString;

        _application = await _builder.BuildAsync(timeout.Token);
        await _application.StartAsync(timeout.Token);
        await _application.ResourceNotifications
            .WaitForResourceHealthyAsync("webfrontend", timeout.Token);

        WebFrontendEndpoint = _application.GetEndpoint("webfrontend");
    }

    private static string LoadTestConnectionString()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile(
                "appsettings.json",
                optional: true,
                reloadOnChange: false)
            .AddUserSecrets(
                typeof(server::Program).Assembly,
                optional: false)
            .AddEnvironmentVariables()
            .Build();

        var connectionString =
            configuration.GetConnectionString("TestConnectionString")
            ?? throw new InvalidOperationException(
                "Connection string 'TestConnectionString' was not found.");
        var builder = new NpgsqlConnectionStringBuilder(connectionString);

        if (string.IsNullOrWhiteSpace(builder.Database) ||
            !builder.Database.Contains("test", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Refusing to run end-to-end tests against database '{builder.Database}'. " +
                "The test database name must contain 'test'.");
        }

        return connectionString;
    }

    public async Task DisposeAsync()
    {
        if (_application is not null)
        {
            await _application.DisposeAsync();
        }

        if (_builder is not null)
        {
            await _builder.DisposeAsync();
        }
    }
}
