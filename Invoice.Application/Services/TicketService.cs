using Invoice.Application.DTO;
using Invoice.Model.Entities;
using SharedKernel.Infrastructure.Persistent.Abstraction;

namespace Invoice.Application.Services;

public class TicketService
{
    private readonly IRepo<Ticket> _ticketRepository;
    private readonly IUOW _uow;

    public TicketService(IRepo<Ticket> ticketRepository, IUOW uow)
    {
        _ticketRepository = ticketRepository;
        _uow = uow;
    }
    public async Task<Ticket> CreateTicketAsync(CreateTicketDTO createTicketDTO)
    {
        throw new NotImplementedException();
    }
}
