namespace HouseFlow.Core.Entities;

public class HouseMember
{
    public Guid Id { get; set; }
    public Guid HouseId { get; set; }
    public Guid UserId { get; set; }
    public HouseRole Role { get; set; }
    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;
    public DateTime CreatedAt { get; set; }
    public DateTime? AcceptedAt { get; set; }

    // Navigation properties
    public House? House { get; set; }
    public User? User { get; set; }
}

public enum HouseRole
{
    Owner,
    Collaborator,
    Tenant
}

public enum InvitationStatus
{
    Pending,
    Accepted,
    Declined
}
