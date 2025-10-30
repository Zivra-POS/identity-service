namespace IdentityService.Shared.DTOs.Request.Role;

public class RoleClaimRequest
{
    public Guid? Id { get; set; }
    public Guid RoleId { get; set; }
    public string ClaimType { get; set; } = default!;
    public string ClaimValue { get; set; } = default!;
}

