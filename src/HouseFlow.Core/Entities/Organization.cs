namespace HouseFlow.Core.Entities;

public class Organization
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public bool IsDefault { get; set; } // True for auto-created default orgs
    public SubscriptionStatus SubscriptionStatus { get; set; } = SubscriptionStatus.Free;
    public DateTime? SubscriptionStartDate { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }
    public string? StripeCustomerId { get; set; }
    public string? StripeSubscriptionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public Guid OwnerId { get; set; }
    public User? Owner { get; set; }
    public ICollection<House> Houses { get; set; } = new List<House>();
}

public enum SubscriptionStatus
{
    Free,
    Active,
    PastDue,
    Canceled
}
