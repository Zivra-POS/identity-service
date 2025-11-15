using ZivraFramework.Core.Models;

namespace IdentityService.Shared.DTOs.Response.Auth;

public class ForgotPasswordResponse : BaseDto
{
    public string Name { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}