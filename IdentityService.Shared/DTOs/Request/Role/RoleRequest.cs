namespace IdentityService.Shared.DTOs.Request.Role;

public class RoleRequest
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public DateTime CreDate { get; set; }
    public string? CreBy { get; set; }
    public string? CreIpAddress { get; set; }
}

