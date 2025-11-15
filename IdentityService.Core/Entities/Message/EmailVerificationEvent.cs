namespace IdentityService.Core.Entities.Message;

public class EmailVerificationEventMessage
{
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required string Username { get; set; }
    public required string VerificationCode { get; set; }
}