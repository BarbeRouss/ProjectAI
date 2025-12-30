using System.ComponentModel.DataAnnotations;

namespace HouseFlow.Application.DTOs;

public record RegisterRequestDto(
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
    string Email,

    [Required(ErrorMessage = "Password is required")]
    [StringLength(255, MinimumLength = 12, ErrorMessage = "Password must be between 12 and 255 characters")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{12,}$",
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character")]
    string Password,

    [Required(ErrorMessage = "Name is required")]
    [StringLength(255, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 255 characters")]
    string Name
);

public record LoginRequestDto(
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    string Email,

    [Required(ErrorMessage = "Password is required")]
    string Password
);

public record RefreshTokenRequestDto(
    [Required(ErrorMessage = "Refresh token is required")]
    string RefreshToken
);

public record AuthResponseDto(
    string Token,
    int ExpiresIn,
    UserDto User,
    string? RefreshToken = null,
    Guid? FirstHouseId = null
);

public record UserDto(Guid Id, string Email, string Name);

public record RevokeTokenRequestDto(
    [Required(ErrorMessage = "Token is required")]
    string Token
);
