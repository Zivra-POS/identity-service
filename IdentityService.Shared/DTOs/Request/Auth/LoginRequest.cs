namespace IdentityService.Shared.DTOs.Request.Auth;

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public DateTime? ModDate { get; set; }
    public string? ModBy { get; set; }
    public string? ModIpAddress { get; set; }
}