using System;

namespace IdentityService.Shared.DTOs.RoleClaim;

public class RoleClaimResponse
{
    public Guid Id { get; set; }
    public Guid RoleId { get; set; }
    public string ClaimType { get; set; } = default!;
    public string ClaimValue { get; set; } = default!;
    public DateTime CreDate { get; set; }
    public DateTime? ModDate { get; set; }
}

