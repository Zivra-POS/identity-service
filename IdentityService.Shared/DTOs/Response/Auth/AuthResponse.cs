namespace IdentityService.Shared.DTOs.Response.Auth;

public class AuthResponse
{
    public Guid Id { get; set; } 
    public required string FullName { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public IEnumerable<string> Roles { get; set; } = [];
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}