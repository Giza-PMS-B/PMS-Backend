using System;
using Booking.Model.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Infrastructure.Persistent.Abstraction;
using SharedKernel.MessageBus.Abstraction;
using Site.Model.Shared;

namespace Booking.Application.EventHandlers;

public class SiteCreatedEventHandler : IMessageHandler<SiteCreatedEvent>
{
    private readonly IRepo<Model.Entities.Site> _siteRepository;
    private readonly IUOW _uow;
    private readonly ILogger<SiteCreatedEventHandler> _logger;




    public SiteCreatedEventHandler(IRepo<Model.Entities.Site> siteRepository, IUOW uow, ILogger<SiteCreatedEventHandler> logger)
    {
        _siteRepository = siteRepository;
        _uow = uow;
        _logger = logger;
    }

    public async Task HandleAsync(SiteCreatedEvent message, CancellationToken ct)
    {
        // Create site in Booking service's database
        _logger.LogInformation("Message come with name in arabic =  {Name}", message.NameAr);

        var site = new Model.Entities.Site
        {
            Id = message.SiteId,
            NameEn = message.NameEn,
            NameAr = message.NameAr,
            Path = message.Path,
            IsLeaf = message.IsLeaf,
            PricePerHour = message.PricePerHour,
            IntegrationCode = message.IntegrationCode,
            NumberOfSolts = message.NumberOfSolts,
        };

        await _siteRepository.AddAsync(site);
        await _uow.SaveChangesAsync();
    }
}