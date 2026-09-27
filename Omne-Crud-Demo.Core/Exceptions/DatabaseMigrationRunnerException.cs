namespace Omne_Crud_Demo.Core.Exceptions;

public abstract class DatabaseMigrationRunnerException : Exception
{
    protected DatabaseMigrationRunnerException(string message)
        : base(message)
    {
    }

    protected DatabaseMigrationRunnerException(
        string message,
        Exception innerException)
        : base(message, innerException)
    {
    }
}
