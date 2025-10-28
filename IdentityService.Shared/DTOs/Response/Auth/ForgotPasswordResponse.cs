namespace IdentityService.Shared.DTOs.Response;

public class ForgotPasswordResponse
{
    public string Name { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}