namespace IdentityService.Shared.DTOs.Request.Auth;

public class ForgotPasswordRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public string? Origin { get; set; }
    public DateTime CreDate { get; set; }
    public string? CreBy { get; set; }
    public string? CreIpAddress { get; set; }
    public DateTime? ModDate { get; set; }
    public string? ModBy { get; set; }
    public string? ModIpAddress { get; set; }
}

