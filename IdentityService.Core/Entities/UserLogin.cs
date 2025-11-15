using ZivraFramework.Core.Models;

namespace IdentityService.Core.Entities;

public class UserLogin : BaseEntity
{
    public Guid UserId { get; set; }
    public string LoginProvider { get; set; } = default!;
    public string ProviderKey { get; set; } = default!;
    public string? ProviderDisplayName { get; set; }

    public User? User { get; set; }
}
