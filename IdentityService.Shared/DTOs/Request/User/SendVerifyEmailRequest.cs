using ZivraFramework.Core.Models;

namespace IdentityService.Shared.DTOs.Request.User;

public class SendVerifyEmailRequest : BaseDto
{
    public required string Email { get; set; }
    public bool IsSend { get; set; }
}