using Omne_Crud_Demo.Domain;
using OmneCrudDemo.Server.Events;

namespace Omne_Crud_Demo.Presentation.Tests.Events;

public sealed class ProductEventStreamTests
{
    [Fact]
    public async Task HandleAsync_ShouldBroadcastProductOperationsToSubscriber()
    {
        var stream = new ProductEventStream();
        using var subscription = stream.Subscribe();

        await stream.HandleAsync(new ProductCreatedDomainEvent(
            "Keyboard", 99.90m, "Mechanical keyboard", "SKU-001"));
        await stream.HandleAsync(new ProductUpdatedDomainEvent(
            10, "Gaming keyboard", 129.90m, "RGB keyboard", "SKU-001"));
        await stream.HandleAsync(new ProductDeletedDomainEvent(
            10, "Gaming keyboard", "SKU-001"));

        var notifications = new List<ProductEventNotification>();
        while (subscription.Reader.TryRead(out var notification))
            notifications.Add(notification);

        Assert.Collection(
            notifications,
            created =>
            {
                Assert.Equal("created", created.Operation);
                Assert.Null(created.ProductId);
                Assert.Equal("SKU-001", created.Sku);
            },
            updated =>
            {
                Assert.Equal("updated", updated.Operation);
                Assert.Equal(10, updated.ProductId);
                Assert.Equal("Gaming keyboard", updated.Name);
            },
            deleted =>
            {
                Assert.Equal("deleted", deleted.Operation);
                Assert.Equal(10, deleted.ProductId);
            });
    }

    [Fact]
    public async Task DisposedSubscription_ShouldStopReceivingNotifications()
    {
        var stream = new ProductEventStream();
        var subscription = stream.Subscribe();
        subscription.Dispose();

        await stream.HandleAsync(new ProductDeletedDomainEvent(
            10, "Keyboard", "SKU-001"));

        Assert.False(subscription.Reader.TryRead(out _));
        Assert.True(subscription.Reader.Completion.IsCompleted);
    }
}
