namespace Omne_Crud_Demo.Abstractions;

public abstract class AuditableEntity : EntityBase
{
    public DateTime CreatedAt { get; protected set; }

    public DateTime? UpdatedAt { get; protected set; }

    public void MarkAsCreated(DateTime when)
    {
        CreatedAt = when;
    }

    public void MarkAsUpdated(DateTime? when = null)
    {
        UpdatedAt = when ?? DateTime.UtcNow;
    }
}
