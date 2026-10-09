using WoodLounge.Application.Auth;
using WoodLounge.Application.Common.Security;
using WoodLounge.Domain.Identity;

namespace WoodLounge.Application.Auth;

public class AuthService(
    IAuthRepository authRepository,
    IJwtTokenService jwtService,
    IPasswordHasher passwordHasher) : IAuthService
{
    public async Task<AuthResponseDto> SignUpAsync(SignUpDto request, CancellationToken ct = default)
    {
        // Check if user already exists
        var existingUser = await authRepository.FindByEmailAsync(request.Email, ct);
        if (existingUser != null)
            throw new InvalidOperationException("User with this email already exists");

        // Create new user
        var user = new User
        {
            Email = request.Email,
            FullName = request.FullName,
            PasswordHash = passwordHasher.Hash(request.Password),
            Role = "customer",
            IsActive = true,
            EmailVerifiedAt = DateTimeOffset.UtcNow
        };

        await authRepository.AddUserAsync(user, ct);

        // Auto-login after signup
        var accessToken = jwtService.GenerateAccessToken(user.Id, user.Role, user.Email, user.Phone);
        var refreshToken = jwtService.GenerateRefreshToken();

        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = passwordHasher.Hash(refreshToken),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(30)
        };
        await authRepository.AddRefreshTokenAsync(refreshTokenEntity, ct);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            User = new UserDto
            {
                Id = user.Id,
                Phone = user.Phone,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role
            }
        };
    }

    public async Task<AuthResponseDto> SignInWithEmailPasswordAsync(EmailPasswordSignInDto request, CancellationToken ct = default)
    {
        var user = await authRepository.FindByEmailAsync(request.Email, ct);
        if (user == null || user.PasswordHash == null)
            throw new UnauthorizedAccessException("Invalid credentials");

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Account is inactive");

        var accessToken = jwtService.GenerateAccessToken(user.Id, user.Role, user.Email, user.Phone);
        var refreshToken = jwtService.GenerateRefreshToken();

        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = passwordHasher.Hash(refreshToken),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(30)
        };
        await authRepository.AddRefreshTokenAsync(refreshTokenEntity, ct);

        user.LastLoginAt = DateTimeOffset.UtcNow;
        await authRepository.UpdateUserAsync(user, ct);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            User = new UserDto
            {
                Id = user.Id,
                Phone = user.Phone,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role
            }
        };
    }

    public async Task<AuthResponseDto> RequestOtpAsync(OtpRequestDto request, System.Net.IPAddress? ip = null, CancellationToken ct = default)
    {
        // Generate OTP code
        var code = new Random().Next(100000, 999999).ToString();
        Console.WriteLine($"[DEV MODE] OTP for {request.Phone}: {code}");

        // Create OTP code in database
        var otpCode = new OtpCode
        {
            Phone = request.Phone,
            CodeHash = passwordHasher.Hash(code),
            Purpose = "login",
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10),
            Ip = ip
        };

        await authRepository.AddOtpCodeAsync(otpCode, ct);

        // Return placeholder response (OTP sent via SMS in production)
        return new AuthResponseDto
        {
            AccessToken = "otp_sent",
            RefreshToken = "verify_otp_to_get_tokens",
            User = new UserDto { Id = Guid.Empty, Phone = request.Phone, Role = "customer" }
        };
    }

    public async Task<AuthResponseDto> VerifyOtpAsync(OtpVerifyDto request, CancellationToken ct = default)
    {
        // Validate OTP format
        if (request.Code.Length != 6)
            throw new ArgumentException("Invalid OTP code");

        // Find or create user
        var user = await authRepository.FindByPhoneAsync(request.Phone, ct);
        if (user == null)
        {
            user = new User
            {
                Phone = request.Phone,
                Role = "customer",
                PhoneVerifiedAt = DateTimeOffset.UtcNow,
                IsActive = true
            };
            await authRepository.AddUserAsync(user, ct);
        }

        // Generate tokens
        var accessToken = jwtService.GenerateAccessToken(user.Id, user.Role, user.Email, user.Phone);
        var refreshToken = jwtService.GenerateRefreshToken();

        // Store refresh token
        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = passwordHasher.Hash(refreshToken),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(30)
        };
        await authRepository.AddRefreshTokenAsync(refreshTokenEntity, ct);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            User = new UserDto
            {
                Id = user.Id,
                Phone = user.Phone,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role
            }
        };
    }

    public async Task<AuthResponseDto> AuthenticateWithGoogleAsync(GoogleAuthDto request, CancellationToken ct = default)
    {
        // TODO: Implement Google OAuth validation
        throw new NotImplementedException("Google auth not yet implemented");
    }

    public async Task<AuthResponseDto> AdminLoginAsync(AdminLoginDto request, CancellationToken ct = default)
    {
        var user = await authRepository.FindByEmailAsync(request.Email, ct);
        if (user == null || user.PasswordHash == null || !user.Role.StartsWith("admin"))
            throw new UnauthorizedAccessException("Invalid credentials");

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials");

        var accessToken = jwtService.GenerateAccessToken(user.Id, user.Role, user.Email, user.Phone);
        var refreshToken = jwtService.GenerateRefreshToken();

        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = passwordHasher.Hash(refreshToken),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(30)
        };
        await authRepository.AddRefreshTokenAsync(refreshTokenEntity, ct);

        user.LastLoginAt = DateTimeOffset.UtcNow;
        await authRepository.UpdateUserAsync(user, ct); // Updates user

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            User = new UserDto
            {
                Id = user.Id,
                Phone = user.Phone,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role
            }
        };
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto request, CancellationToken ct = default)
    {
        // TODO: Implement token refresh with rotation
        throw new NotImplementedException("Token refresh not yet implemented");
    }

    public async Task LogoutAsync(Guid userId, string? refreshToken = null, CancellationToken ct = default)
    {
        await authRepository.RevokeRefreshTokenAsync(userId, ct);
    }
}
