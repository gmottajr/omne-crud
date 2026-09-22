using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Omne_Crud_Demo.Abstractions;
using Omne_Crud_Demo.Application.Products.Events;
using Omne_Crud_Demo.Domain;
using Omne_Crud_Demo.Infrastructure.Events;
using Omne_Crud_Demo.Infrastructure.Persistence.Data;

namespace Omne_Crud_Demo.Infrastructure.Tests.Integration;

[Collection(InfrastructureIntegrationCollection.Name)]
public sealed class DomainEventIntegrationTests : IAsyncLifetime
{
    private readonly InfrastructureDatabaseFixture _fixture;

    public DomainEventIntegrationTests(InfrastructureDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync()
    {
        return _fixture.ResetDatabaseAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldPersistDispatchAndClearProductCreatedEvent()
    {
        await using var serviceProvider = CreateServiceProvider();
        var dispatcher = serviceProvider.GetRequiredService<IDomainEventDispatcher>();
        await using var context = _fixture.CreateDbContext(dispatcher);

        var product = new Product(
            "Keyboard",
            99.90m,
            "Mechanical keyboard",
            Sku.Create("EVENT-001"));

        context.Products.Add(product);

        await context.SaveChangesAsync();

        var handler = serviceProvider.GetRequiredService<RecordingProductCreatedHandler>();
        var handledEvent = Assert.Single(handler.HandledEvents);

        Assert.Equal("Keyboard", handledEvent.Name);
        Assert.Equal(99.90m, handledEvent.Price);
        Assert.Equal("Mechanical keyboard", handledEvent.Description);
        Assert.Equal("EVENT-001", handledEvent.Sku);
        Assert.Empty(product.DomainEvents);

        var persistedProduct = await context.Products
            .AsNoTracking()
            .SingleAsync();

        Assert.Equal(product.Id, persistedProduct.Id);
        Assert.Equal("EVENT-001", persistedProduct.Sku.Value);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldPropagateHandlerFailureAndKeepEventPending()
    {
        await using var serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddSingleton<IDomainEventHandler<ProductCreatedDomainEvent>, ThrowingProductCreatedHandler>()
            .AddSingleton<IDomainEventDispatcher, DomainEventDispatcher>()
            .BuildServiceProvider();
        var dispatcher = serviceProvider.GetRequiredService<IDomainEventDispatcher>();
        await using var context = _fixture.CreateDbContext(dispatcher);

        var product = new Product(
            "Mouse",
            49.90m,
            "Gaming mouse",
            Sku.Create("EVENT-002"));

        context.Products.Add(product);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => context.SaveChangesAsync());

        Assert.Equal(ThrowingProductCreatedHandler.ErrorMessage, exception.Message);
        Assert.Single(product.DomainEvents);
        Assert.Equal(1, await context.Products.CountAsync());
    }

    private static ServiceProvider CreateServiceProvider()
    {
        return new ServiceCollection()
            .AddLogging()
            .AddSingleton<IDomainEventHandler<ProductCreatedDomainEvent>, ProductCreatedDomainEventHandler>()
            .AddSingleton<RecordingProductCreatedHandler>()
            .AddSingleton<IDomainEventHandler<ProductCreatedDomainEvent>>(provider =>
                provider.GetRequiredService<RecordingProductCreatedHandler>())
            .AddSingleton<IDomainEventDispatcher, DomainEventDispatcher>()
            .BuildServiceProvider();
    }

    private sealed class RecordingProductCreatedHandler
        : IDomainEventHandler<ProductCreatedDomainEvent>
    {
        public List<ProductCreatedDomainEvent> HandledEvents { get; } = [];

        public Task HandleAsync(
            ProductCreatedDomainEvent domainEvent,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            HandledEvents.Add(domainEvent);
            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingProductCreatedHandler
        : IDomainEventHandler<ProductCreatedDomainEvent>
    {
        public const string ErrorMessage = "The test handler failed.";

        public Task HandleAsync(
            ProductCreatedDomainEvent domainEvent,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(ErrorMessage);
        }
    }
}
