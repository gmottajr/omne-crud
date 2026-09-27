using Microsoft.EntityFrameworkCore;
using Omne_Crud_Demo.Core.Exceptions;
using Omne_Crud_Demo.Infrastructure.Persistence.Data;

namespace Omne_Crud_Demo.DatabaseMigrator;

public static class DatabaseMigrationRunner
{
    public static async Task MigrateAsync(AppDbContext dbContext, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        try
        {
            var migrations = dbContext.Database.GetMigrations();

            if (!migrations.Any())
            {
                throw new NoDatabaseMigrationsFoundException();
            }

            await dbContext.Database.MigrateAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (DatabaseMigrationRunnerException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new DatabaseMigrationExecutionException(exception);
        }
    }
}
