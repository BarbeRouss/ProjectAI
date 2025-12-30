using System.ComponentModel.DataAnnotations;
using HouseFlow.Core.Entities;

namespace HouseFlow.Application.DTOs;

public record CreateHouseRequestDto(
    [Required(ErrorMessage = "House name is required")]
    [StringLength(255, MinimumLength = 1, ErrorMessage = "House name must be between 1 and 255 characters")]
    string Name,

    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    string? Address,

    [StringLength(20, ErrorMessage = "Zip code cannot exceed 20 characters")]
    [RegularExpression(@"^[A-Z0-9\s-]+$", ErrorMessage = "Invalid zip code format")]
    string? ZipCode,

    [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
    string? City
);

public record HouseDto(Guid Id, string Name, string? Address, string? ZipCode, string? City, HouseRole Role);

public record HouseDetailDto(Guid Id, string Name, string? Address, string? ZipCode, string? City, HouseRole Role, IEnumerable<HouseMemberDto> Members);

public record InviteMemberRequestDto(
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
    string Email,

    [EnumDataType(typeof(HouseRole), ErrorMessage = "Invalid role")]
    HouseRole Role
);

public record HouseMemberDto(Guid Id, string Email, HouseRole Role, InvitationStatus Status);
