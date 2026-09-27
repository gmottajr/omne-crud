namespace Omne_Crud_Demo.Core.Exceptions;

public sealed class NoDatabaseMigrationsFoundException
    : DatabaseMigrationRunnerException
{
    public NoDatabaseMigrationsFoundException()
        : base("No EF Core migrations were found for AppDbContext.")
    {
    }
}
