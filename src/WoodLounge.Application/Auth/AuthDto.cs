namespace WoodLounge.Application.Auth;

public record EmailPasswordSignInDto(string Email, string Password);
public record SignUpDto(string FullName, string Email, string Password);
public record OtpRequestDto(string Phone);
public record OtpVerifyDto(string Phone, string Code);
public record GoogleAuthDto(string IdToken);
public record AdminLoginDto(string Email, string Password);
public record RefreshTokenDto(string RefreshToken);

public record AuthResponseDto
{
    public string AccessToken { get; init; } = null!;
    public string RefreshToken { get; init; } = null!;
    public UserDto User { get; init; } = null!;
}

public record UserDto
{
    public Guid Id { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public string FullName { get; init; } = null!;
    public string Role { get; init; } = null!;
}
