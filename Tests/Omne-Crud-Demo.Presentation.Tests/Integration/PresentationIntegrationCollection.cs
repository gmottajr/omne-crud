namespace Omne_Crud_Demo.Presentation.Tests.Integration;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class PresentationIntegrationCollection : ICollectionFixture<PresentationWebApplicationFactory>
{
    public const string Name = "Presentation Integration Tests";
}
