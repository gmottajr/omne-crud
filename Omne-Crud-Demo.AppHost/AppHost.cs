var builder = DistributedApplication.CreateBuilder(args);

var productsDatabase = builder.AddProductsDatabase();
var databaseMigrator = builder.AddDatabaseMigrator(productsDatabase);
var server = builder.AddServer(productsDatabase, databaseMigrator);

var webfrontend = builder.AddViteApp("webfrontend", "../frontend")
    .WithReference(server)
    .WaitFor(server);

server.PublishWithContainerFiles(webfrontend, "wwwroot");

builder.Build().Run();
