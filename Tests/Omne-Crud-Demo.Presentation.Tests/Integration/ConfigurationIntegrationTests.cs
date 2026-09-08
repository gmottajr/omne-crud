using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Omne_Crud_Demo.Presentation.Tests.Integration;

public sealed class ConfigurationIntegrationTests
{
    [Fact]
    public void Configuration_Should_Load_TestConnectionString_From_Server_UserSecrets()
    {
        // Arrange
        var configuration = BuildConfiguration();

        // Act
        var connectionString =
            configuration.GetConnectionString("TestConnectionString");

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(connectionString));
    }

    [Fact]
    public void TestConnectionString_Should_Point_To_Test_Database()
    {
        // Arrange
        var configuration = BuildConfiguration();

        // Act
        var connectionString =
            configuration.GetConnectionString("TestConnectionString");

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(connectionString));

        var builder =
            new NpgsqlConnectionStringBuilder(connectionString);

        Assert.False(string.IsNullOrWhiteSpace(builder.Database));

        Assert.Contains(
            "test",
            builder.Database,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Configuration_Should_Load_DefaultConnection_From_Server_UserSecrets()
    {
        // Arrange
        var configuration = BuildConfiguration();

        // Act
        var connectionString =
            configuration.GetConnectionString("DefaultConnection");

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(connectionString));
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        return new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile(
                "appsettings.json",
                optional: true,
                reloadOnChange: false)
            .AddUserSecrets(
                typeof(Program).Assembly,
                optional: false)
            .AddEnvironmentVariables()
            .Build();
    }
}
