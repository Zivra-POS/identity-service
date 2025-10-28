using ZivraFramework.Core.Interfaces;

namespace IdentityService.Core.Entities;

public class UserLogin : IBaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }
    public string LoginProvider { get; set; } = default!;
    public string ProviderKey { get; set; } = default!;
    public string? ProviderDisplayName { get; set; }

    public DateTime CreDate { get; set; }
    public string? CreBy { get; set; }
    public string? CreIpAddress { get; set; }
    public DateTime? ModDate { get; set; }
    public string? ModBy { get; set; }
    public string? ModIpAddress { get; set; }

    public User? User { get; set; }
}
