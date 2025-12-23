using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Invoice.Application.DTO;
using Invoice.Model.Entities;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Infrastructure.Persistent.Abstraction;

namespace Invoice.Application.Services;

public class InvoiceService
{
    private readonly IRepo<Model.Entities.Invoice> _invoiceRepository;
    private readonly IRepo<Ticket> _ticketRepository;
    private readonly IUOW _uow;

    public InvoiceService(IRepo<Model.Entities.Invoice> invoiceRepository, IRepo<Ticket> ticketRepository, IUOW uow)
    {
        _invoiceRepository = invoiceRepository;
        _ticketRepository = ticketRepository;
        _uow = uow;
    }
    public async Task<Model.Entities.Invoice> CreateInvoiceAsync(CreateInvoiceDTO createInvoiceDTO)
    {
        await ValidateTicketExistsAsync(createInvoiceDTO.TicketId);

        var invoice = new Model.Entities.Invoice
        {
            Id = Guid.NewGuid(),
            HtmlDocumentPath = createInvoiceDTO.HtmlDocumentPath,
            TaxAmount = 10,
            TotalAmountBeforeTax = CalcAmountBeforeTax(createInvoiceDTO.TotalAmountAfterTax, 10),
            TotalAmountAfterTax = createInvoiceDTO.TotalAmountAfterTax,
            TicketSerialNumber = createInvoiceDTO.TicketSerialNumber,
            TicketId = createInvoiceDTO.TicketId
        };

        await _invoiceRepository.AddAsync(invoice);
        await _uow.SaveChangesAsync();

        return invoice;
    }
    private decimal CalcAmountBeforeTax(decimal totalPrice, decimal Amount)
    {
        return (10 / 100) * totalPrice + totalPrice;
    }

    private async Task ValidateTicketExistsAsync(Guid ticketId)
    {
        var ticketExists = await _ticketRepository.GetAll()
            .AnyAsync(t => t.Id == ticketId);

        if (!ticketExists)
            throw new ValidationException($"Ticket with ID {ticketId} does not exist.");
    }
}
