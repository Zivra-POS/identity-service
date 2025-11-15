using ZivraFramework.Core.Models;

namespace IdentityService.Core.Entities;

public class UserToken : BaseEntity
{
    public Guid UserId { get; set; }
    public string LoginProvider { get; set; } = "Local";
    public string Name { get; set; } = default!;
    public string Value { get; set; } = default!;

    public User? User { get; set; }
}
