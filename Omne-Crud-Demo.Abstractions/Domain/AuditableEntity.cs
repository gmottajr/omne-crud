namespace Omne_Crud_Demo.Abstractions;

public abstract class AuditableEntity : EntityBase
{
    public DateTime CreatedAt { get; protected set; }

    public DateTime? UpdatedAt { get; protected set; }

    protected AuditableEntity()
    {
        CreatedAt = DateTime.UtcNow;
    }

    protected void MarkAsUpdated(DateTime? when = null)
    {
        UpdatedAt = when ?? DateTime.UtcNow;
    }
}
