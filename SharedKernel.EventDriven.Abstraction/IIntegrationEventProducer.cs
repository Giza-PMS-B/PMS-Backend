using SharedKernel.MessageBus.Abstraction;

namespace SharedKernel.EventDriven.Abstraction
{
    public interface IIntegrationEventProducer
    {
        public void Enqueue<TIntegrationEvent>(TIntegrationEvent message)
            where TIntegrationEvent : IntegrationEvent;
    }
}
