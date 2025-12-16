using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedKernel.MessageBus.Abstraction;

namespace SharedKernel.EventDriven.Abstraction
{
    public interface IIntegrationEventQueue : IIntegrationEventProducer
    {
        public IEnumerable<IntegrationEvent> GetAllEvents();
        public void Reset();
    }
}
