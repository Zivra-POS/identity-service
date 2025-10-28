using ZivraFramework.Core.Interfaces;

namespace IdentityService.Shared.DTOs.Request.User;

public class SendVerifyEmailRequest
{
    public required Guid UserId { get; set; }
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required string Username { get; set; }
    public bool IsSend { get; set; }
    public DateTime CreDate { get; set; }
    public string? CreBy { get; set; }
    public string? CreIpAddress { get; set; }
    public DateTime? ModDate { get; set; }
    public string? ModBy { get; set; }
    public string? ModIpAddress { get; set; }
}