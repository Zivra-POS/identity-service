namespace IdentityService.Shared.DTOs.Request.UserRole;

public class CreateUserRoleRequest
{
    public string? UserId { get; set; }
    public List<Guid>? RoleIds { get; set; }
}