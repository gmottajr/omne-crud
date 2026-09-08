using System;
using System.Collections.Generic;
using System.Text;

namespace Omne_Crud_Demo.Infrastructure.Tests.Integration;

[CollectionDefinition(Name,DisableParallelization = true)]
public sealed class InfrastructureIntegrationCollection: ICollectionFixture<InfrastructureDatabaseFixture>
{
    public const string Name = "Infrastructure Integration Tests";
}
