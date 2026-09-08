using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Omne_Crud_Demo.Application.Tests.Integration;

public sealed class ConfigurationIntegrationTests
{
    [Fact]
    public void Configuration_Should_Load_TestConnectionString_From_UserSecrets()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
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

        // Act
        var connectionString =
            configuration.GetConnectionString("TestConnectionString");

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(connectionString));
    }

    [Fact]
    public void TestConnectionString_Should_Point_To_A_Test_Database()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
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

        // Act
        var connectionString =
            configuration.GetConnectionString("TestConnectionString");

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(connectionString));

        var builder =
            new Npgsql.NpgsqlConnectionStringBuilder(connectionString);

        Assert.False(string.IsNullOrWhiteSpace(builder.Database));

        Assert.Contains(
            "test",
            builder.Database,
            StringComparison.OrdinalIgnoreCase);
    }
}
