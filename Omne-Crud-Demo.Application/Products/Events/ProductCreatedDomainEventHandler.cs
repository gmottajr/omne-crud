using Omne_Crud_Demo.Abstractions;
using Omne_Crud_Demo.Domain;

namespace Omne_Crud_Demo.Application.Products.Events;

public sealed class ProductCreatedDomainEventHandler : IDomainEventHandler<ProductCreatedDomainEvent>
{
    public Task HandleAsync(
        ProductCreatedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.CompletedTask;
    }
}
