namespace SharedKernel.MessageBus.Abstraction
{
    public interface IMessagePublisher
    {
        Task PublishAsync<TEvent>(
            TEvent message, int topicNUmber,
            CancellationToken cancellationToken = default)
            where TEvent : IntegrationEvent;
    }
}
