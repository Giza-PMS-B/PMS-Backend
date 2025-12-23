using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.EventDriven.Abstraction;
using SharedKernel.Infrastructure.Persistent.Abstraction;
using SharedKernel.MessageBus.Abstraction;

namespace SharedKernel.Infrastructure.Persistent
{
    public class UOW : IUOW
    {
        //very important to uncomment these lines when you want to use events
        private readonly DbContext _dbContext;
        private readonly IMessagePublisher _messagePublisher;
        private readonly IIntegrationEventQueue _messageQueue;
        private readonly ILogger<UOW>? _logger;

        public UOW(DbContext dbContext, IMessagePublisher messagePublisher, IIntegrationEventQueue messageQueue, ILogger<UOW>? logger = null)
        {
            _dbContext = dbContext;
            this._messagePublisher = messagePublisher;
            _messageQueue = messageQueue;
            _logger = logger;
        }

        public async Task SaveChangesAsync()
        {
            _logger?.LogDebug("Starting SaveChangesAsync");

            await _dbContext.SaveChangesAsync();
            _logger?.LogDebug("Database changes saved successfully");

            var events = _messageQueue.GetAllEvents().ToList();
            _logger?.LogInformation("Publishing {EventCount} integration events in memory", events.Count);

            foreach (var integrationEvent in events)
            {
                await _messagePublisher.PublishAsync(integrationEvent);
            }

            _messageQueue.Reset();
            _logger?.LogInformation("Integration events published and queue reset");
        }
    }
}
