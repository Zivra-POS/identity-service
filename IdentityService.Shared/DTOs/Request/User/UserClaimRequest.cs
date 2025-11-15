using System;
using ZivraFramework.Core.Models;

namespace IdentityService.Shared.DTOs.Request.User;

public class UserClaimRequest : BaseDto
{
    public Guid UserId { get; set; }
    public string ClaimType { get; set; } = default!;
    public string ClaimValue { get; set; } = default!;
}
