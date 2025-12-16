namespace SharedKernel.MessageBus.Abstraction
{
    public interface IMessageHandler<in TEvent>
    where TEvent : IntegrationEvent
    {
        Task HandleAsync(TEvent message, CancellationToken ct);
    }
}
