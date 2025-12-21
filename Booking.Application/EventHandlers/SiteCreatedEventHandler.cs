using System;
using Booking.Model.Entities;
using SharedKernel.Infrastructure.Persistent.Abstraction;
using SharedKernel.MessageBus.Abstraction;
using Site.Model.Shared.Events;

namespace Booking.Application.EventHandlers;

public class SiteCreatedEventHandler : IMessageHandler<SiteCreatedEvent>
{
    private readonly IRepo<Model.Entities.Site> _siteRepository;
    private readonly IUOW _uow;

    public SiteCreatedEventHandler(IRepo<Model.Entities.Site> siteRepository, IUOW uow)
    {
        _siteRepository = siteRepository;
        _uow = uow;
    }

    public async Task HandleAsync(SiteCreatedEvent message, CancellationToken ct)
    {
        // // Check if site already exists (idempotency)
        // var existingSite = _siteRepository.GetAll().Where(s => s.Id == message.SiteId).FirstOrDefault();
        // if (existingSite != null) return;

        // Create site in Booking service's database
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