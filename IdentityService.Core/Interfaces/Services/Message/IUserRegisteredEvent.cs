using IdentityService.Core.Entities;
using IdentityService.Core.Entities.Message;

namespace IdentityService.Core.Interfaces.Services.Message;

public interface IUserRegisteredEvent
{
    Task PublishAsync(UserRegisteredEventMessage message);
}