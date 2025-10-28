using System;

namespace IdentityService.Shared.DTOs.UserClaim;

public class UserClaimRequest
{
    public Guid? Id { get; set; }
    public Guid UserId { get; set; }
    public string ClaimType { get; set; } = default!;
    public string ClaimValue { get; set; } = default!;
}

