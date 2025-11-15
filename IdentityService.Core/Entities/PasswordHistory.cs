using ZivraFramework.Core.Models;

namespace IdentityService.Core.Entities;

public class PasswordHistory : BaseEntity
{
    public Guid UserId { get; set; }
    public string PasswordHash { get; set; } = default!;
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}
