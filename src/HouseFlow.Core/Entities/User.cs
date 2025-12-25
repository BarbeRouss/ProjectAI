namespace HouseFlow.Core.Entities;

public class User
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public required string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public Organization? DefaultOrganization { get; set; }
    public Guid? DefaultOrganizationId { get; set; }
    public ICollection<HouseMember> HouseMemberships { get; set; } = new List<HouseMember>();
}
