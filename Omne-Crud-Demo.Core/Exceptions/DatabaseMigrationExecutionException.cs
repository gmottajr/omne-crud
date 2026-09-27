namespace Omne_Crud_Demo.Core.Exceptions;

public sealed class DatabaseMigrationExecutionException
    : DatabaseMigrationRunnerException
{
    public DatabaseMigrationExecutionException(Exception innerException)
        : base(
            "An error occurred while applying EF Core database migrations.",
            innerException)
    {
    }
}
