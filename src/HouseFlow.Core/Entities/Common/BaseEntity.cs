namespace HouseFlow.Core.Entities.Common;

/// <summary>
/// Base entity class with soft delete and audit trail support
/// </summary>
public abstract class BaseEntity : ISoftDeletable, IAuditable
{
    public Guid Id { get; set; }

    // Audit trail
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public Guid? ModifiedBy { get; set; }

    // Soft delete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
}
