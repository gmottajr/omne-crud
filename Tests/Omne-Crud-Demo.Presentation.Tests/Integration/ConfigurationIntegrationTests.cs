using Npgsql;

namespace Omne_Crud_Demo.Presentation.Tests.Integration;

[Collection(PresentationIntegrationCollection.Name)]
public sealed class ConfigurationIntegrationTests
{
    private readonly PresentationWebApplicationFactory _factory;

    public ConfigurationIntegrationTests(
        PresentationWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public void TestDatabase_ShouldUseTheConfiguredLocalTestDatabase()
    {
        var builder = new NpgsqlConnectionStringBuilder(
            _factory.ConnectionString);

        Assert.Contains("test", builder.Database, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(5432, builder.Port);
    }
}
