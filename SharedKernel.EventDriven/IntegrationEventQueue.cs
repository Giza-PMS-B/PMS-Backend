using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedKernel.EventDriven.Abstraction;
using SharedKernel.MessageBus.Abstraction;

namespace SharedKernel.EventDriven
{
    public class IntegrationEventQueue : IIntegrationEventQueue
    {
        private ConcurrentQueue<IntegrationEvent> messageQueue;

        public IntegrationEventQueue(ConcurrentQueue<IntegrationEvent> messages)
        {
            this.messageQueue = messages;
        }

        public void Enqueue<TIntegrationEvent>(TIntegrationEvent message) where TIntegrationEvent : IntegrationEvent
        {
            messageQueue.Enqueue(message);
        }

        public IEnumerable<IntegrationEvent> GetAllEvents()
        {
            return [.. messageQueue];
        }

        public void Reset()
        {
            this.messageQueue.Clear();
        }
    }
}
