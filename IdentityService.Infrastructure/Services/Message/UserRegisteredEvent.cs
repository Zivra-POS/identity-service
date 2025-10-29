using IdentityService.Core.Entities.Message;
using IdentityService.Core.Interfaces.Services.Message;
using ZivraFramework.Core.Interfaces;

namespace IdentityService.Infrastructure.Services.Message;

public class UserRegisteredEvent : IUserRegisteredEvent
{
    private readonly IKafkaProducer _producer;

    public UserRegisteredEvent(IKafkaProducer producer)
    {
        _producer = producer;
    }

    #region PublishAsync
    public async Task PublishAsync(UserRegisteredEventMessage message)
    {
        await _producer.ProduceAsync("user-registered-events", message);
    }
    #endregion
}