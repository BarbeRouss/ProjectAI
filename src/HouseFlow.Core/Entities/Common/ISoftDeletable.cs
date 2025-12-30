namespace HouseFlow.Core.Entities.Common;

/// <summary>
/// Interface for entities that support soft delete
/// </summary>
public interface ISoftDeletable
{
    /// <summary>
    /// Indicates whether the entity has been soft deleted
    /// </summary>
    bool IsDeleted { get; set; }

    /// <summary>
    /// The date and time when the entity was soft deleted
    /// </summary>
    DateTime? DeletedAt { get; set; }

    /// <summary>
    /// The user who soft deleted the entity
    /// </summary>
    Guid? DeletedBy { get; set; }
}
