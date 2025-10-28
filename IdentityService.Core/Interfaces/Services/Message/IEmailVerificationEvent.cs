using IdentityService.Core.Entities;
using IdentityService.Core.Entities.Message;

namespace IdentityService.Core.Interfaces.Services.Message;

public interface IEmailVerificationEvent
{
    Task PublishAsync(EmailVerificationEventMessage message);
}