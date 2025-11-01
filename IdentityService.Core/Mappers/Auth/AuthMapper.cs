using IdentityService.Shared.DTOs.Response.Auth;

namespace IdentityService.Core.Mappers;

public static class AuthMapper
{
    public static AuthResponse ToAuthResponse(Entities.User user, IEnumerable<string> roles, string? token, string? refreshToken, string? verifyToken = null) 
    {
        return new AuthResponse
        {
            Id = user.Id,
            FullName = user.FullName ?? string.Empty,
            Username = user.Username,
            Email = user.Email,
            Roles = roles,
            Token = token ?? string.Empty,
            RefreshToken = refreshToken ?? string.Empty,
            VerifyToken = verifyToken
        };
    }
}