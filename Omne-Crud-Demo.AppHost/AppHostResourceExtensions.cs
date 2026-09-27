using Aspire.Hosting.ApplicationModel;

internal static class AppHostResourceExtensions
{
    private const string DefaultConnectionName = "DefaultConnection";

    public static IResourceBuilder<PostgresDatabaseResource> AddProductsDatabase(
        this IDistributedApplicationBuilder builder)
    {
        var postgres = builder.AddPostgres("postgres")
            .WithDataVolume();

        return postgres.AddDatabase(
            name: "productsdb",
            databaseName: "omne");
    }

    public static IResourceBuilder<ProjectResource> AddDatabaseMigrator(
        this IDistributedApplicationBuilder builder,
        IResourceBuilder<PostgresDatabaseResource> productsDatabase)
    {
        return builder
            .AddProject<Projects.Omne_Crud_Demo_DatabaseMigrator>(
                "database-migrator")
            .WithReference(
                productsDatabase,
                connectionName: DefaultConnectionName)
            .WaitFor(productsDatabase);
    }

    public static IResourceBuilder<ProjectResource> AddServer(
        this IDistributedApplicationBuilder builder,
        IResourceBuilder<PostgresDatabaseResource> productsDatabase,
        IResourceBuilder<ProjectResource> databaseMigrator)
    {
        return builder
            .AddProject<Projects.Omne_Crud_Demo_Server>("server")
            .WithReference(
                productsDatabase,
                connectionName: DefaultConnectionName)
            .WaitForCompletion(databaseMigrator)
            .WithHttpHealthCheck("/health")
            .WithExternalHttpEndpoints();
    }
}
