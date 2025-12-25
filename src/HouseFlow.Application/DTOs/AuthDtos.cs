namespace HouseFlow.Application.DTOs;

public record RegisterRequestDto(string Email, string Password, string Name);
public record LoginRequestDto(string Email, string Password);

public record AuthResponseDto(string Token, int ExpiresIn, UserDto User, Guid? FirstHouseId = null);

public record UserDto(Guid Id, string Email, string Name);
