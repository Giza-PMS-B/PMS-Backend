using Booking.Application.DTO;
using Booking.Model.Entities;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Infrastructure.Persistent.Abstraction;

namespace Booking.Application.Services;

public class TicketService
{
    private readonly IRepo<Ticket> _ticketRepository;
    private readonly IRepo<Model.Entities.Site> _siteRepository;
    private readonly IUOW _uow;

    public TicketService(IRepo<Ticket> ticketRepository, IRepo<Model.Entities.Site> siteRepository, IUOW uow)
    {
        _ticketRepository = ticketRepository;
        _siteRepository = siteRepository;
        _uow = uow;
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
        await _uow.SaveChangesAsync();

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
