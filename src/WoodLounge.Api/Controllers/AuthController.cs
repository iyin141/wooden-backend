using Microsoft.AspNetCore.Mvc;
using WoodLounge.Application.Auth;

namespace WoodLounge.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("signup")]
    public async Task<ActionResult<AuthResponseDto>> SignUp([FromBody] SignUpDto request)
    {
        var result = await _authService.SignUpAsync(request);
        return Ok(result);
    }

    [HttpPost("signin")]
    public async Task<ActionResult<AuthResponseDto>> SignIn([FromBody] EmailPasswordSignInDto request)
    {
        var result = await _authService.SignInWithEmailPasswordAsync(request);
        return Ok(result);
    }

    [HttpPost("otp/request")]
    public async Task<ActionResult<AuthResponseDto>> RequestOtp([FromBody] OtpRequestDto request)
    {
        var ip = HttpContext.Connection.RemoteIpAddress;
        var result = await _authService.RequestOtpAsync(request, ip);
        return Ok(result);
    }

    [HttpPost("otp/verify")]
    public async Task<ActionResult<AuthResponseDto>> VerifyOtp([FromBody] OtpVerifyDto request)
    {
        var result = await _authService.VerifyOtpAsync(request);
        return Ok(result);
    }

    [HttpPost("google")]
    public async Task<ActionResult<AuthResponseDto>> GoogleAuth([FromBody] GoogleAuthDto request)
    {
        var result = await _authService.AuthenticateWithGoogleAsync(request);
        return Ok(result);
    }

    [HttpPost("admin/login")]
    public async Task<ActionResult<AuthResponseDto>> AdminLogin([FromBody] AdminLoginDto request)
    {
        var result = await _authService.AdminLoginAsync(request);
        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshTokenDto request)
    {
        var result = await _authService.RefreshTokenAsync(request);
        return Ok(result);
    }

    [HttpPost("logout")]
    public async Task<ActionResult> Logout([FromBody] RefreshTokenDto? request)
    {
        // Extract user ID from JWT (need middleware to populate User)
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            await _authService.LogoutAsync(userId, request?.RefreshToken);
        }
        return Ok();
    }
}
