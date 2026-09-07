namespace Omne_Crud_Demo.Abstractions;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}
