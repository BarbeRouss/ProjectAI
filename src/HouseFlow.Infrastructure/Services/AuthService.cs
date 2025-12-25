using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HouseFlow.Application.DTOs;
using HouseFlow.Application.Interfaces;
using HouseFlow.Core.Entities;
using HouseFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using BCryptNet = BCrypt.Net.BCrypt;

namespace HouseFlow.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly HouseFlowDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(HouseFlowDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        // Check if user already exists
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            throw new InvalidOperationException("User with this email already exists");
        }

        // Create user
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            Name = request.Name,
            PasswordHash = BCryptNet.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);

        // Create default organization
        var organization = new Organization
        {
            Id = Guid.NewGuid(),
            Name = $"{request.Name}'s Organization",
            IsDefault = true,
            OwnerId = user.Id,
            SubscriptionStatus = SubscriptionStatus.Free,
            CreatedAt = DateTime.UtcNow
        };

        _context.Organizations.Add(organization);

        user.DefaultOrganizationId = organization.Id;

        // Create default first house
        var house = new House
        {
            Id = Guid.NewGuid(),
            Name = "Ma Maison",
            OrganizationId = organization.Id,
            CreatedAt = DateTime.UtcNow
        };

        _context.Houses.Add(house);

        // Add user as owner of the house
        var houseMember = new HouseMember
        {
            Id = Guid.NewGuid(),
            HouseId = house.Id,
            UserId = user.Id,
            Role = HouseRole.Owner,
            Status = InvitationStatus.Accepted,
            CreatedAt = DateTime.UtcNow,
            AcceptedAt = DateTime.UtcNow
        };

        _context.HouseMembers.Add(houseMember);

        await _context.SaveChangesAsync();

        var token = GenerateJwtToken(user.Id, user.Email);

        return new AuthResponseDto(
            token,
            3600,
            new UserDto(user.Id, user.Email, user.Name),
            house.Id
        );
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || !BCryptNet.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        var token = GenerateJwtToken(user.Id, user.Email);

        return new AuthResponseDto(
            token,
            3600,
            new UserDto(user.Id, user.Email, user.Name)
        );
    }

    public string GenerateJwtToken(Guid userId, string email)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured")));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
