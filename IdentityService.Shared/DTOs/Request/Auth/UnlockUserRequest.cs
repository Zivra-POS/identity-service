using ZivraFramework.Core.Models;

namespace IdentityService.Shared.DTOs.Request.Auth;

public class UnlockUserRequest : BaseDto
{
    public Guid UserId { get; set; }
    public string? Reason { get; set; }
}
