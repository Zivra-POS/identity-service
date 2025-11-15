using System;
using ZivraFramework.Core.Models;

namespace IdentityService.Shared.DTOs.Response.User;

public class UserClaimResponse : BaseDto
{
    public Guid UserId { get; set; }
    public string ClaimType { get; set; } = default!;
    public string ClaimValue { get; set; } = default!;
}
