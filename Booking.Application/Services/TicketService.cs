using Booking.Application.DTO;
using Booking.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SharedKernel.EventDriven.Abstraction;
using SharedKernel.Infrastructure.Persistent.Abstraction;

namespace Booking.Application.Services;

public class TicketService
{
    private readonly IRepo<Ticket> _ticketRepository;
    private readonly IRepo<Model.Entities.Site> _siteRepository;
    private readonly IUOW _uow;
    private readonly ILogger<TicketService> _logger;
    private readonly IIntegrationEventProducer _eventProducer;

    public TicketService(IRepo<Ticket> ticketRepository, IRepo<Model.Entities.Site> siteRepository, IUOW uow, ILogger<TicketService> logger, IIntegrationEventProducer eventProducer)
    {
        _ticketRepository = ticketRepository;
        _siteRepository = siteRepository;
        _logger = logger;
        _uow = uow;
        _eventProducer = eventProducer;
    }
    public async Task<Ticket> CreateTicketAsync(CreateTicketDTO createTicketDTO)

    {
        await ValidateSiteExistsAsync(createTicketDTO.SiteId);


        var bookingFrom = DateTime.UtcNow;
        var bookingTo = bookingFrom.AddHours(createTicketDTO.NoOfHours);

        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            SiteName = createTicketDTO.SiteName,
            PlateNumber = createTicketDTO.PlateNumber,
            PhoneNumber = createTicketDTO.PhoneNumber,
            BookingFrom = bookingFrom,
            BookingTo = bookingTo,
            TotalPrice = createTicketDTO.TotalPrice,
            SiteId = createTicketDTO.SiteId
        };

        await _ticketRepository.AddAsync(ticket);

        var BookingCreatedEvent = new BookingCreatedEvent
        {
            Id = ticket.Id,
            SiteName = ticket.SiteName,
            PhoneNumber = ticket.PhoneNumber,
            PlateNumber = ticket.PlateNumber,
            BookingFrom = ticket.BookingFrom,
            BookingTo = ticket.BookingTo,
            TotalPrice = ticket.TotalPrice,
        };
        _eventProducer.Enqueue(BookingCreatedEvent);
        _logger.LogInformation("Enqueued Booking Created event for Ticket  {TicketId}", ticket.Id);

        await _uow.SaveChangesAsync(2);

        return ticket;
    }
    private async Task ValidateSiteExistsAsync(Guid siteId)
    {
        var exists = await _siteRepository
            .GetAll()
            .AnyAsync(s => s.Id == siteId);

        if (!exists)
            throw new Exception("Site not found");
    }
}
