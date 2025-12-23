namespace SharedKernel.MessageBus.Abstraction
{
    public interface IMessagePublisher
    {
        Task PublishAsync<TEvent>(
            TEvent message, CancellationToken cancellationToken = default)
            where TEvent : IntegrationEvent;
    }
}
