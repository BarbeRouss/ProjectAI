using HouseFlow.Core.Entities;

namespace HouseFlow.Application.DTOs;

public record CreateHouseRequestDto(string Name, string? Address, string? ZipCode, string? City);

public record HouseDto(Guid Id, string Name, string? Address, string? ZipCode, string? City, HouseRole Role);

public record HouseDetailDto(Guid Id, string Name, string? Address, string? ZipCode, string? City, HouseRole Role, IEnumerable<HouseMemberDto> Members);

public record InviteMemberRequestDto(string Email, HouseRole Role);

public record HouseMemberDto(Guid Id, string Email, HouseRole Role, InvitationStatus Status);
