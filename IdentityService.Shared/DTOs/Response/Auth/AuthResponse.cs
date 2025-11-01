namespace IdentityService.Shared.DTOs.Response.Auth;

public class AuthResponse
{
    public Guid Id { get; set; } 
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public IEnumerable<string> Roles { get; set; } = System.Array.Empty<string>();
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string? VerifyToken { get; set; }
}