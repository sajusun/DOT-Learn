using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderPulse.Application.Common.Interfaces;

namespace OrderPulse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthController(IJwtTokenGenerator jwtTokenGenerator)
    {
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public sealed record LoginRequest(string Email, string Password);
    public sealed record AuthResponse(string AccessToken, string RefreshToken, string TokenType, int ExpiresInSeconds);

    /// <summary>
    /// Login and obtain a JWT Access Token and Refresh Token.
    /// In Laravel: Like Route::post('/login', [AuthController::class, 'login']) with Sanctum.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        // For demonstration/reference testing, authenticate demo customer or admin
        if (request.Email.Equals("admin@enterprise.com", StringComparison.OrdinalIgnoreCase) && request.Password == "AdminSecret123!")
        {
            var adminId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var token = _jwtTokenGenerator.GenerateToken(adminId, request.Email, "Admin");
            var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();

            return Ok(new AuthResponse(token, refreshToken, "Bearer", 3600));
        }

        if (request.Email.Equals("alex.engineer@enterprise.com", StringComparison.OrdinalIgnoreCase) && request.Password == "SecretPassword123!")
        {
            var customerId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var token = _jwtTokenGenerator.GenerateToken(customerId, request.Email, "Customer");
            var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();

            return Ok(new AuthResponse(token, refreshToken, "Bearer", 3600));
        }

        return Unauthorized(new ProblemDetails
        {
            Title = "Authentication Failed",
            Detail = "Invalid email or password.",
            Status = StatusCodes.Status401Unauthorized
        });
    }
}
