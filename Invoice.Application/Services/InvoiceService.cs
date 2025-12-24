using System.ComponentModel.DataAnnotations;
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
        //await ValidateTicketExistsAsync(createInvoiceDTO.TicketId);

        var invoice = new Model.Entities.Invoice
        {
            Id = Guid.NewGuid(),
            TaxAmount = 10,
            TotalAmountBeforeTax = CalcAmountBeforeTax(createInvoiceDTO.TotalAmountAfterTax, 10),
            TotalAmountAfterTax = createInvoiceDTO.TotalAmountAfterTax,
            TicketSerialNumber = GenerateSerial(),
            TicketId = createInvoiceDTO.TicketId
        };
        var invoiceHtmlDocumentPath = await SaveInvoiceHtmlDoc(createInvoiceDTO.HtmlDocument);
        invoice.HtmlDocumentPath = invoiceHtmlDocumentPath;

        var ticket = await GetTicket(createInvoiceDTO.TicketId);

        var InvoiceERBDTO = CrateInvoiceERBDTO(invoice, ticket);

        await _invoiceRepository.AddAsync(invoice);
        await _uow.SaveChangesAsync();

        return invoice;
    }
    private InvoiceERBDTO CrateInvoiceERBDTO(Model.Entities.Invoice invoice, Ticket ticket)
    {
        return new InvoiceERBDTO
        {
            InvoiceId = invoice.Id,
            TicketId = invoice.TicketId,
            TicketSerial = invoice.TicketSerialNumber,
            BookingFrom = ticket.BookingFrom,
            BookingTo = ticket.BookingTo,
            NumOfHours = calcNumOfHours(ticket.BookingTo, ticket.BookingFrom),
            PlateNumber = ticket.PlateNumber,
            TotalAmountBeforeTax = CalcAmountBeforeTax(ticket.TotalPrice, invoice.TaxAmount),
            TotalAmountAfterTax = ticket.TotalPrice,
            TicketSerialNumber = invoice.TicketSerialNumber,
            InvoiceDocPath = invoice.HtmlDocumentPath
        };
    }
    private async Task ValidateTicketExistsAsync(Guid ticketId)
    {
        var ticketExists = await _ticketRepository.GetAll()
            .AnyAsync(t => t.Id == ticketId);

        if (!ticketExists)
            throw new ValidationException($"Ticket with ID {ticketId} does not exist.");
    }
    private decimal CalcAmountBeforeTax(decimal totalPrice, decimal Amount)
    {
        return (10 / 100) * totalPrice + totalPrice;
    }
    private async Task<Ticket> GetTicket(Guid ticketId)
    {
        return await _ticketRepository.GetAll().Where(t => t.Id == ticketId).FirstOrDefaultAsync();
    }

    private int calcNumOfHours(DateTime to, DateTime from)
    {
        TimeSpan duration = to - from;
        double numOfHours = duration.TotalHours;
        return (int)numOfHours;
    }
    private string GenerateSerial()
    {
        var random = new Random();
        return random.Next(0, 1_000_000_000).ToString("D9");
    }


    private async Task<string> SaveInvoiceHtmlDoc(string htmlDocument)
    {
        return "https://5023/docs/1";
    }

}

