using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SharedKernel.EventDriven.Abstraction;
using SharedKernel.Infrastructure.Persistent.Abstraction;
using SharedKernel.MessageBus.Abstraction;

namespace SharedKernel.Infrastructure.Persistent
{
    public class UOW : IUOW
    {
        private readonly DbContext _dbContext;
        private readonly IMessagePublisher _messagePublisher;
        private readonly IIntegrationEventQueue _messageQueue;
        public UOW(DbContext dbContext, IMessagePublisher messagePublisher, IIntegrationEventQueue messageQueue)
        {
            _dbContext = dbContext;
            this._messagePublisher = messagePublisher;
            _messageQueue = messageQueue;
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();

            var events = _messageQueue.GetAllEvents().ToList();
            foreach (var integrationEvent in events)
            {
                await _messagePublisher.PublishAsync(integrationEvent);
            }

            _messageQueue.Reset();
        }
    }
}
