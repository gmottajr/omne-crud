namespace Omne_Crud_Demo.Abstractions;

public abstract class AggregateRoot<TKey> : AuditableEntity
{
    public TKey Id { get; protected set; } = default!;

}