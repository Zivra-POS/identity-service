using ZivraFramework.Core.Models;

namespace IdentityService.Shared.DTOs.Response.Role;

public class RoleResponse : BaseDto
{
    public string Name { get; set; } = default!;
    public string NormalizedName { get; set; } = default!;
    public string? Description { get; set; }
}
