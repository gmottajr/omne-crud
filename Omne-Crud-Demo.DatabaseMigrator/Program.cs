using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Omne_Crud_Demo.DatabaseMigrator;
using Omne_Crud_Demo.Infrastructure.Persistence.Data;
using Omne_Crud_Demo.IoC;


var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole();

ILogger? logger = null;

try
{
    builder.Services.AddPersistence(builder.Configuration);

    using var host = builder.Build();

    logger = host.Services
        .GetRequiredService<ILoggerFactory>()
        .CreateLogger("DatabaseMigrator");

    logger.LogInformation("Applying pending database migrations.");

    await using var scope = host.Services.CreateAsyncScope();

    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    await DatabaseMigrationRunner.MigrateAsync(dbContext);

    logger.LogInformation(
        "Database migrations applied successfully.");

    return 0;
}
catch (Exception exception)
{
    if (logger is null)
    {
        Console.Error.WriteLine(
            $"Database migration failed.{Environment.NewLine}{exception}");
    }
    else
    {
        logger.LogCritical(
            exception,
            "Database migration failed.");
    }

    return 1;
}
