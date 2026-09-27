using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Omne_Crud_Demo.Infrastructure.Persistence.Data;
using Omne_Crud_Demo.IoC;


var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddPersistence(builder.Configuration);

using var host = builder.Build();

var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseMigrator");

try
{
    logger.LogInformation("Applying pending database migrations.");

    await using var scope = host.Services.CreateAsyncScope();

    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    await dbContext.Database.MigrateAsync();

    logger.LogInformation(
        "Database migrations applied successfully.");

    return 0;
}
catch (Exception exception)
{
    logger.LogCritical(
        exception,
        "Database migration failed.");

    return 1;
}