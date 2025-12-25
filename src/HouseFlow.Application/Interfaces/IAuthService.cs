using HouseFlow.Application.DTOs;

namespace HouseFlow.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    string GenerateJwtToken(Guid userId, string email);
}
