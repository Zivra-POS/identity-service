using ZivraFramework.Core.Models;

namespace IdentityService.Core.Entities;

public class UserSecurityLog : BaseEntity
{
    public Guid UserId { get; set; }
    public string Action { get; set; } = default!; 
    public string? Description { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    public User? User { get; set; }
}
