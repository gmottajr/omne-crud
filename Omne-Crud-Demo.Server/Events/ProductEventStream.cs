using System.Collections.Concurrent;
using System.Threading.Channels;
using Omne_Crud_Demo.Abstractions;
using Omne_Crud_Demo.Domain;

namespace OmneCrudDemo.Server.Events;

public sealed class ProductEventStream :
    IDomainEventHandler<ProductCreatedDomainEvent>,
    IDomainEventHandler<ProductUpdatedDomainEvent>,
    IDomainEventHandler<ProductDeletedDomainEvent>
{
    private readonly ConcurrentDictionary<Guid, Channel<ProductEventNotification>> _subscribers = new();

    public ProductEventSubscription Subscribe()
    {
        var subscriberId = Guid.NewGuid();
        var channel = Channel.CreateBounded<ProductEventNotification>(new BoundedChannelOptions(100)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = false
        });

        _subscribers[subscriberId] = channel;
        return new ProductEventSubscription(channel.Reader, () => RemoveSubscriber(subscriberId));
    }

    public Task HandleAsync(
        ProductCreatedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Publish(new ProductEventNotification(
            Guid.NewGuid(),
            "created",
            null,
            domainEvent.Name,
            domainEvent.Sku,
            domainEvent.OccurredOn));
        return Task.CompletedTask;
    }

    public Task HandleAsync(
        ProductUpdatedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Publish(new ProductEventNotification(
            Guid.NewGuid(),
            "updated",
            domainEvent.ProductId,
            domainEvent.Name,
            domainEvent.Sku,
            domainEvent.OccurredOn));
        return Task.CompletedTask;
    }

    public Task HandleAsync(
        ProductDeletedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Publish(new ProductEventNotification(
            Guid.NewGuid(),
            "deleted",
            domainEvent.ProductId,
            domainEvent.Name,
            domainEvent.Sku,
            domainEvent.OccurredOn));
        return Task.CompletedTask;
    }

    private void Publish(ProductEventNotification notification)
    {
        foreach (var channel in _subscribers.Values)
            channel.Writer.TryWrite(notification);
    }

    private void RemoveSubscriber(Guid subscriberId)
    {
        if (_subscribers.TryRemove(subscriberId, out var channel))
            channel.Writer.TryComplete();
    }
}

public sealed class ProductEventSubscription(
    ChannelReader<ProductEventNotification> reader,
    Action unsubscribe) : IDisposable
{
    private bool _disposed;

    public ChannelReader<ProductEventNotification> Reader { get; } = reader;

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        unsubscribe();
    }
}
