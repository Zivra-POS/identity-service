using ZivraFramework.Core.Models;

namespace IdentityService.Shared.DTOs.User;

public class UserWithRolesDto : BaseDto
{
    public string Username { get; set; } = default!;
    public string? FullName { get; set; }
    public List<string> RoleNames { get; set; } = new();
}