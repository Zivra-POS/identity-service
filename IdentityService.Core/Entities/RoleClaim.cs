using ZivraFramework.Core.Models;

namespace IdentityService.Core.Entities;

public class RoleClaim : BaseEntity
{
    public Guid RoleId { get; set; }
    public string ClaimType { get; set; } = default!;
    public string ClaimValue { get; set; } = default!;

    public Role? Role { get; set; }
}
