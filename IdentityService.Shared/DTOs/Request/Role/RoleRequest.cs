using ZivraFramework.Core.Models;

namespace IdentityService.Shared.DTOs.Request.Role;

public class RoleRequest : BaseDto
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}
