using HouseFlow.Application.DTOs;
using HouseFlow.Application.Interfaces;
using HouseFlow.Core.Entities;
using HouseFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HouseFlow.Infrastructure.Services;

public class HouseService : IHouseService
{
    private readonly HouseFlowDbContext _context;

    public HouseService(HouseFlowDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<HouseDto>> GetUserHousesAsync(Guid userId)
    {
        var houses = await _context.HouseMembers
            .Where(hm => hm.UserId == userId && hm.Status == InvitationStatus.Accepted)
            .Include(hm => hm.House)
            .Select(hm => new HouseDto(
                hm.House!.Id,
                hm.House.Name,
                hm.House.Address,
                hm.House.ZipCode,
                hm.House.City,
                hm.Role
            ))
            .ToListAsync();

        return houses;
    }

    public async Task<HouseDetailDto> GetHouseDetailsAsync(Guid houseId, Guid userId)
    {
        var houseMember = await _context.HouseMembers
            .Where(hm => hm.HouseId == houseId && hm.UserId == userId && hm.Status == InvitationStatus.Accepted)
            .Include(hm => hm.House)
                .ThenInclude(h => h!.Members)
                    .ThenInclude(m => m.User)
            .FirstOrDefaultAsync();

        if (houseMember == null)
        {
            throw new UnauthorizedAccessException("Access denied to this house");
        }

        var members = houseMember.House!.Members
            .Select(m => new HouseMemberDto(
                m.Id,
                m.User!.Email,
                m.Role,
                m.Status
            ));

        return new HouseDetailDto(
            houseMember.House.Id,
            houseMember.House.Name,
            houseMember.House.Address,
            houseMember.House.ZipCode,
            houseMember.House.City,
            houseMember.Role,
            members
        );
    }

    public async Task<HouseDto> CreateHouseAsync(CreateHouseRequestDto request, Guid userId)
    {
        var user = await _context.Users
            .Include(u => u.DefaultOrganization)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user?.DefaultOrganization == null)
        {
            throw new InvalidOperationException("User does not have an organization");
        }

        // Check house quota
        var houseCount = await _context.Houses
            .CountAsync(h => h.OrganizationId == user.DefaultOrganizationId);

        if (houseCount >= 1 && user.DefaultOrganization.SubscriptionStatus == SubscriptionStatus.Free)
        {
            throw new InvalidOperationException("Free plan allows only 1 house. Please upgrade to Premium.");
        }

        var house = new House
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Address = request.Address,
            ZipCode = request.ZipCode,
            City = request.City,
            OrganizationId = user.DefaultOrganizationId!.Value,
            CreatedAt = DateTime.UtcNow
        };

        _context.Houses.Add(house);

        // Add creator as owner
        var houseMember = new HouseMember
        {
            Id = Guid.NewGuid(),
            HouseId = house.Id,
            UserId = userId,
            Role = HouseRole.Owner,
            Status = InvitationStatus.Accepted,
            CreatedAt = DateTime.UtcNow,
            AcceptedAt = DateTime.UtcNow
        };

        _context.HouseMembers.Add(houseMember);

        await _context.SaveChangesAsync();

        return new HouseDto(house.Id, house.Name, house.Address, house.ZipCode, house.City, HouseRole.Owner);
    }

    public async Task InviteMemberAsync(Guid houseId, InviteMemberRequestDto request, Guid userId)
    {
        // Check if user is owner
        var houseMember = await _context.HouseMembers
            .FirstOrDefaultAsync(hm => hm.HouseId == houseId && hm.UserId == userId);

        if (houseMember?.Role != HouseRole.Owner)
        {
            throw new UnauthorizedAccessException("Only house owners can invite members");
        }

        // Find or create invited user
        var invitedUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (invitedUser == null)
        {
            // In a real app, send invitation email and create user on acceptance
            // For now, we'll just throw an exception
            throw new InvalidOperationException("User must be registered first");
        }

        // Check if already a member
        var existingMember = await _context.HouseMembers
            .FirstOrDefaultAsync(hm => hm.HouseId == houseId && hm.UserId == invitedUser.Id);

        if (existingMember != null)
        {
            throw new InvalidOperationException("User is already a member");
        }

        var newMember = new HouseMember
        {
            Id = Guid.NewGuid(),
            HouseId = houseId,
            UserId = invitedUser.Id,
            Role = request.Role,
            Status = InvitationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.HouseMembers.Add(newMember);
        await _context.SaveChangesAsync();
    }
}
