namespace WoodLounge.Application.Auth;

public interface IAuthService
{
    Task<AuthResponseDto> SignUpAsync(SignUpDto request, CancellationToken ct = default);
    Task<AuthResponseDto> SignInWithEmailPasswordAsync(EmailPasswordSignInDto request, CancellationToken ct = default);
    Task<AuthResponseDto> RequestOtpAsync(OtpRequestDto request, System.Net.IPAddress? ip = null, CancellationToken ct = default);
    Task<AuthResponseDto> VerifyOtpAsync(OtpVerifyDto request, CancellationToken ct = default);
    Task<AuthResponseDto> AuthenticateWithGoogleAsync(GoogleAuthDto request, CancellationToken ct = default);
    Task<AuthResponseDto> AdminLoginAsync(AdminLoginDto request, CancellationToken ct = default);
    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto request, CancellationToken ct = default);
    Task LogoutAsync(Guid userId, string? refreshToken = null, CancellationToken ct = default);
}
