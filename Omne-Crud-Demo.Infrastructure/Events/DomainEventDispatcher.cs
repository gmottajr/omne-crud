using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Omne_Crud_Demo.Abstractions;

namespace Omne_Crud_Demo.Infrastructure.Events;

public sealed class DomainEventDispatcher(
    IServiceProvider serviceProvider,
    ILogger<DomainEventDispatcher> logger) : IDomainEventDispatcher
{
    public async Task DispatchAsync(
        IReadOnlyCollection<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
            var handlers = serviceProvider.GetServices(handlerType).Cast<object>().ToArray();

            if (handlers.Length == 0)
            {
                logger.LogDebug(
                    "No handlers registered for domain event {DomainEventType}.",
                    domainEvent.GetType().Name);
                continue;
            }

            foreach (var handler in handlers)
            {
                try
                {
                    var handleMethod = handlerType.GetMethod(nameof(IDomainEventHandler<IDomainEvent>.HandleAsync))
                        ?? throw new InvalidOperationException(
                            $"Handler type '{handler.GetType().Name}' does not expose HandleAsync.");

                    var task = (Task?)handleMethod.Invoke(handler, [domainEvent, cancellationToken]);
                    if (task is not null)
                        await task;
                }
                catch (Exception ex)
                {
                    var actualException = ex is System.Reflection.TargetInvocationException { InnerException: not null } invocationException
                        ? invocationException.InnerException
                        : ex;

                    logger.LogError(
                        actualException,
                        "Handler {HandlerType} failed for domain event {DomainEventType}.",
                        handler.GetType().Name,
                        domainEvent.GetType().Name);

                    throw actualException;
                }
            }
        }
    }
}
