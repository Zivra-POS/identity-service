using ZivraFramework.Core.Models;

namespace IdentityService.Shared.DTOs.Request.Auth;

public class ForgotPasswordRequest : BaseDto
{
    public string Email { get; set; } = string.Empty;
    public string? Origin { get; set; }
}
