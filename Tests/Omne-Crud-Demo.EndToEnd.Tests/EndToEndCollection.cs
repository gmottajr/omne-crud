namespace Omne_Crud_Demo.EndToEnd.Tests;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class EndToEndCollection
    : ICollectionFixture<AppHostEndToEndFixture>
{
    public const string Name = "Aspire End-to-End Tests";
}
