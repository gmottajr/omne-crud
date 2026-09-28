using Aspire.Hosting.ApplicationModel;

internal static class AppHostResourceExtensions
{
    private const string DefaultConnectionName = "DefaultConnection";

    public static IResourceBuilder<IResourceWithConnectionString> AddProductsDatabase(
        this IDistributedApplicationBuilder builder)
    {
        var connectionString = builder.AddParameter(
            "productsdb-connection",
            () => builder.Configuration["ConnectionStrings:productsdb"]
                ?? throw new DistributedApplicationException(
                    "Connection string 'productsdb' was not configured."),
            secret: true);

        return builder.AddConnectionString(
            "productsdb",
            ReferenceExpression.Create($"{connectionString}"));
    }

    public static IResourceBuilder<ProjectResource> AddDatabaseMigrator(
        this IDistributedApplicationBuilder builder,
        IResourceBuilder<IResourceWithConnectionString> productsDatabase)
    {
        return builder
            .AddProject<Projects.Omne_Crud_Demo_DatabaseMigrator>(
                "database-migrator")
            .WithReference(
                productsDatabase,
                connectionName: DefaultConnectionName);
    }

    public static IResourceBuilder<ProjectResource> AddServer(
        this IDistributedApplicationBuilder builder,
        IResourceBuilder<IResourceWithConnectionString> productsDatabase,
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
