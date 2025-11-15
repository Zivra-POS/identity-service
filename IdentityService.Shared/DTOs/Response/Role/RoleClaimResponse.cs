using System;
using ZivraFramework.Core.Models;

namespace IdentityService.Shared.DTOs.RoleClaim;

public class RoleClaimResponse : BaseDto
{
    public Guid RoleId { get; set; }
    public string ClaimType { get; set; } = default!;
    public string ClaimValue { get; set; } = default!;
}
