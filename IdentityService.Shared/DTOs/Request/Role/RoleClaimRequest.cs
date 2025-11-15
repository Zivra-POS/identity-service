using ZivraFramework.Core.Models;

namespace IdentityService.Shared.DTOs.Request.Role;

public class RoleClaimRequest : BaseDto
{
    public Guid RoleId { get; set; }
    public string ClaimType { get; set; } = default!;
    public string ClaimValue { get; set; } = default!;
}
