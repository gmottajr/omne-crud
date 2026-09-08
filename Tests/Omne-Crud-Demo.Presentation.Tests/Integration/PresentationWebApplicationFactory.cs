using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Omne_Crud_Demo.Infrastructure.Persistence.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Omne_Crud_Demo.Presentation.Tests.Integration;

public sealed class PresentationWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
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
    }

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
                    _connectionString,
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
}
