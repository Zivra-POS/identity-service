using ZivraFramework.Core.Models;

namespace IdentityService.Shared.DTOs.Response.UserRole;

public class UserRoleResponse : BaseDto
{
    public Guid? UserId { get; set; }
    public Guid? RoleId { get; set; }
    public string? RoleName { get; set; }
}