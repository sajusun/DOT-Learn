namespace OrderPulse.Application.Common.Interfaces;

/// <summary>
/// Abstraction for JWT token generation.
/// In Laravel: Like Laravel Sanctum creating Personal Access Tokens ($user->createToken(...))
/// </summary>
public interface IJwtTokenGenerator
{
    string GenerateToken(Guid userId, string email, string role);
    string GenerateRefreshToken();
}
