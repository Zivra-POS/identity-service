using ZivraFramework.Core.Models;

namespace IdentityService.Shared.DTOs.Request.Auth;

public class ResetPasswordRequest : BaseDto
{
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
