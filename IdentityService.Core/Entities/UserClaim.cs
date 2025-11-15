using ZivraFramework.Core.Models;

namespace IdentityService.Core.Entities;

public class UserClaim : BaseEntity
{
    public Guid UserId { get; set; }
    public string ClaimType { get; set; } = default!;
    public string ClaimValue { get; set; } = default!;

    public User? User { get; set; }
}
