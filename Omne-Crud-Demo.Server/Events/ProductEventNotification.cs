namespace OmneCrudDemo.Server.Events;

public sealed record ProductEventNotification(
    Guid Id,
    string Operation,
    int? ProductId,
    string Name,
    string Sku,
    DateTime OccurredOn);
