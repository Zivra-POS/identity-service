namespace IdentityService.Shared.DTOs.Request.Auth;

public class UnlockUserRequest
{
    public Guid UserId { get; set; }
    public string? Reason { get; set; }
    public string? ModBy { get; set; }
    public string? ModIpAddress { get; set; }
    public DateTime ModDate { get; set; } = DateTime.UtcNow;
}
