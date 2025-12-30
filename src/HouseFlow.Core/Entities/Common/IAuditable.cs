namespace HouseFlow.Core.Entities.Common;

/// <summary>
/// Interface for entities that support audit trail
/// </summary>
public interface IAuditable
{
    /// <summary>
    /// The date and time when the entity was created
    /// </summary>
    DateTime CreatedAt { get; set; }

    /// <summary>
    /// The user who created the entity
    /// </summary>
    Guid? CreatedBy { get; set; }

    /// <summary>
    /// The date and time when the entity was last modified
    /// </summary>
    DateTime? ModifiedAt { get; set; }

    /// <summary>
    /// The user who last modified the entity
    /// </summary>
    Guid? ModifiedBy { get; set; }
}
