using ZivraFramework.Core.Interfaces;

namespace IdentityService.Core.Entities;

public class RoleClaim : IBaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RoleId { get; set; }
    public string ClaimType { get; set; } = default!;
    public string ClaimValue { get; set; } = default!;

    public DateTime CreDate { get; set; }
    public string? CreBy { get; set; }
    public string? CreIpAddress { get; set; }
    public DateTime? ModDate { get; set; }
    public string? ModBy { get; set; }
    public string? ModIpAddress { get; set; }

    public Role? Role { get; set; }
}
