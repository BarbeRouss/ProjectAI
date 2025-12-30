namespace HouseFlow.Core.Entities;

/// <summary>
/// Audit log for tracking all changes to entities
/// </summary>
public class AuditLog
{
    public Guid Id { get; set; }

    /// <summary>
    /// The type of entity that was modified (e.g., "User", "House", "Device")
    /// </summary>
    public required string EntityType { get; set; }

    /// <summary>
    /// The ID of the entity that was modified
    /// </summary>
    public required string EntityId { get; set; }

    /// <summary>
    /// The type of action performed (Create, Update, Delete, SoftDelete, Restore)
    /// </summary>
    public required string Action { get; set; }

    /// <summary>
    /// The user who performed the action
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// The username/email of the user who performed the action
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// The date and time when the action was performed
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// The old values (JSON) before the change
    /// </summary>
    public string? OldValues { get; set; }

    /// <summary>
    /// The new values (JSON) after the change
    /// </summary>
    public string? NewValues { get; set; }

    /// <summary>
    /// The properties that were changed (JSON array)
    /// </summary>
    public string? ChangedProperties { get; set; }

    /// <summary>
    /// The IP address of the user who performed the action
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// The user agent of the client
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Additional context or notes about the change
    /// </summary>
    public string? AdditionalData { get; set; }
}
