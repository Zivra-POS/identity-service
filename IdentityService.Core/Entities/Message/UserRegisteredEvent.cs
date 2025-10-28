namespace IdentityService.Core.Entities.Message;

public class UserRegisteredEventMessage
{
    public required string FullName { get; set; }
    public Guid? UserId { get; set; }
    public string? Email { get; set; }
    public string? Username { get; set; }
    public string? Token { get; set; }
}