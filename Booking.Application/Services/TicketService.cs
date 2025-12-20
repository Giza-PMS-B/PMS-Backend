using Booking.Application.DTO;
using Booking.Model.Entities;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Infrastructure.Persistent.Abstraction;

namespace Booking.Application.Services;

public class TicketService
{
    private readonly IRepo<Ticket> _ticketRepository;
    private readonly IRepo<Site> _siteRepository;
    private readonly IUOW _uow;

    public TicketService(IRepo<Ticket> ticketRepository,IRepo<Site>siteRepository, IUOW uow)
    {
        _ticketRepository = ticketRepository;
        _siteRepository = siteRepository;
        _uow = uow;
    }
    public async Task<Ticket> CreateTicketAsync(CreateTicketDTO createTicketDTO)

    {
        var site = _siteRepository.GetAll().Where(s=>s.Id==createTicketDTO.SiteId);
        if (site == null)
            throw new Exception("Site not found");

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
        await _uow.SaveChangesAsync();

        return ticket;
    }
}
