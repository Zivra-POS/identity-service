using ZivraFramework.Core.Models;

namespace IdentityService.Core.Entities;

public class UserSession : BaseEntity
{
    public Guid UserId { get; set; }
    public string SessionId { get; set; } = Guid.NewGuid().ToString("N");
    public string? DeviceId { get; set; }
    public string? DeviceName { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime? LastSeenAt { get; set; }
    public DateTime? RevokedAt { get; set; }

    public User? User { get; set; }
}
