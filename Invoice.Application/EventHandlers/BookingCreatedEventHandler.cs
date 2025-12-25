using System;
using Invoice.Model.Entities;
using Microsoft.Extensions.Logging;
using Booking.Model.Shared;
using SharedKernel.Infrastructure.Persistent.Abstraction;
using SharedKernel.MessageBus.Abstraction;
using Invoice.Application.Services;

namespace Invoice.Application.EventHandlers;

public class BookingCreatedEventHandler : IMessageHandler<BookingCreatedEvent>
{
    private readonly IRepo<Ticket> _siteRepository;
    private readonly InvoiceService _invoiceService;
    private readonly IUOW _uow;
    private readonly ILogger<BookingCreatedEventHandler> _logger;

    public BookingCreatedEventHandler(IRepo<Ticket> siteRepository, IUOW uow, InvoiceService invoiceService, ILogger<BookingCreatedEventHandler> logger)
    {
        _siteRepository = siteRepository;
        _invoiceService = invoiceService;
        _uow = uow;
        _logger = logger;
    }

    public async Task HandleAsync(BookingCreatedEvent message, CancellationToken ct)
    {
        _logger.LogInformation("Message come with site name  =  {Name}", message.SiteName);

        var ticket = new Ticket
        {
            Id = message.Id,
            SiteName = message.SiteName,
            PhoneNumber = message.PhoneNumber,
            PlateNumber = message.PlateNumber,
            BookingFrom = message.BookingFrom,
            BookingTo = message.BookingTo,
            TotalPrice = message.TotalPrice,
        };

        await _siteRepository.AddAsync(ticket);
        await _uow.SaveChangesAsync();

        await _invoiceService.SendInvoiceToERB(
            new DTO.CreateInvoiceDTO
            {
                TotalAmountAfterTax = ticket.TotalPrice,
                TicketId = ticket.Id,
            }
        );
    }

}
