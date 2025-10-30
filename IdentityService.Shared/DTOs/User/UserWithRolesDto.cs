namespace IdentityService.Shared.DTOs.User;

public class UserWithRolesDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = default!;
    public string? FullName { get; set; }
    public List<string> RoleNames { get; set; } = new();
}