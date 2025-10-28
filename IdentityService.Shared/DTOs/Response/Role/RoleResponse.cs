namespace IdentityService.Shared.DTOs.Response.Role;

public class RoleResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string NormalizedName { get; set; } = default!;
    public string? Description { get; set; }
    public DateTime CreDate { get; set; }
    public DateTime? ModDate { get; set; }
}
