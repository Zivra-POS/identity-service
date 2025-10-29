using IdentityService.Core.Entities.Message;
using IdentityService.Core.Interfaces.Services.Message;
using ZivraFramework.Core.Interfaces;

namespace IdentityService.Infrastructure.Services.Message;

public class EmailVerificationEvent : IEmailVerificationEvent
{
    private readonly IKafkaProducer _producer;

    public EmailVerificationEvent(IKafkaProducer producer)
    {
        _producer = producer;
    }

    #region PublishAsync
    public async Task PublishAsync(EmailVerificationEventMessage message)
    {
        await _producer.ProduceAsync("email-verification-event", message);
    }
    #endregion
}