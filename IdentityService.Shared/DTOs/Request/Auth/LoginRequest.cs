using ZivraFramework.Core.Models;

namespace IdentityService.Shared.DTOs.Request.Auth;

public class LoginRequest : BaseDto
{
    public string UsernameOrEmail { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}