using ZivraFramework.Core.Interfaces;

namespace IdentityService.Core.Entities;

public class UserRole : IBaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }

    public DateTime CreDate { get; set; }
    public string? CreBy { get; set; }
    public string? CreIpAddress { get; set; }
    public DateTime? ModDate { get; set; }
    public string? ModBy { get; set; }
    public string? ModIpAddress { get; set; }

    public User? User { get; set; }
    public Role? Role { get; set; }
}
