using System;
using System.Collections.Generic;
using System.Text;
using Omne_Crud_Demo.Application.Tests.Integration.Fixtures;

namespace Omne_Crud_Demo.Application.Tests.Integration;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class ApplicationIntegrationCollection: ICollectionFixture<ApplicationDatabaseFixture>
{
    public const string Name = "Application Integration Tests";
}