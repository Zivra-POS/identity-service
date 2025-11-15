using ZivraFramework.Core.Models;

namespace IdentityService.Core.Entities;

public class AccessToken : BaseEntity
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = default!;             
    public DateTime Expires { get; set; }
    public DateTime? Revoked { get; set; }
    public string? RevokedByIp { get; set; }             
    public bool IsActive => Revoked == null && DateTime.UtcNow < Expires;

    public User? User { get; set; }
}
