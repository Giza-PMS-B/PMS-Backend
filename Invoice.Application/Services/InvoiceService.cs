using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Invoice.Application.DTO;
using Invoice.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using SharedKernel.Infrastructure.Persistent.Abstraction;

namespace Invoice.Application.Services;

public class InvoiceService
{
    private readonly IRepo<Model.Entities.Invoice> _invoiceRepository;
    private readonly IRepo<Ticket> _ticketRepository;
    private readonly IUOW _uow;
    private readonly IConfiguration _configuration;



    public InvoiceService(IRepo<Model.Entities.Invoice> invoiceRepository, IRepo<Ticket> ticketRepository, IUOW uow, IConfiguration configuration)
    {
        _invoiceRepository = invoiceRepository;
        _ticketRepository = ticketRepository;
        _uow = uow;
        _configuration = configuration;
    }

    public async Task SendInvoiceToERB(CreateInvoiceDTO createInvoiceDTO)
    {
        var invoice = await CreateInvoiceAsync(createInvoiceDTO);
        var ticket = await GetTicket(createInvoiceDTO.TicketId);
        var InvoiceERBDTO = CrateInvoiceERBDTO(invoice, ticket);
        await ERBIntegrationService.SendInvoiceToErpAsync(InvoiceERBDTO);
    }


    public async Task<Model.Entities.Invoice> CreateInvoiceAsync(CreateInvoiceDTO createInvoiceDTO)
    {

        await ValidateTicketExistsAsync(createInvoiceDTO.TicketId);

        var invoice = new Model.Entities.Invoice
        {
            Id = Guid.NewGuid(),
            TaxAmount = 10,
            TotalAmountBeforeTax = CalcAmountBeforeTax(createInvoiceDTO.TotalAmountAfterTax, 10),
            TotalAmountAfterTax = createInvoiceDTO.TotalAmountAfterTax,
            TicketSerialNumber = GenerateSerial(),
            TicketId = createInvoiceDTO.TicketId
        };

        var invoiceHtmlDocument = CreateInvoiceHtmlDocument(invoice);
        invoice.HtmlDocument = invoiceHtmlDocument;

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
            InvoiceHTMLDoc = invoice.HtmlDocument
        };
    }
    private async Task ValidateTicketExistsAsync(Guid ticketId)
    {
        var ticketExists = await _ticketRepository.GetAll()
            .AnyAsync(t => t.Id == ticketId);

        if (!ticketExists)
            throw new ValidationException($"Ticket with ID {ticketId} does not exist.");
    }
    private decimal CalcAmountBeforeTax(decimal totalPrice, decimal TaxAmount)
    {
        return totalPrice - ((TaxAmount / 100) * totalPrice);
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

    // private async Task<string> ProcessInvoiceHtmlDocument(Model.Entities.Invoice invoice)
    // {
    //     string document = CreateInvoiceHtmlDocument(invoice);
    //     string url = await SaveInvoiceHtmlDoc(document, invoice.TicketSerialNumber);
    //     return url;
    // }
    private string CreateInvoiceHtmlDocument(Model.Entities.Invoice invoice)
    {
        return $@"
        <!DOCTYPE html>
        <html lang=""en"">
        <head>
            <meta charset=""UTF-8"">
            <title>Invoice {invoice.TicketSerialNumber}</title>
            <style>
                body {{
                    font-family: Arial, sans-serif;
                    margin: 40px;
                    color: #333;
                }}
                .invoice-box {{
                    max-width: 800px;
                    margin: auto;
                    padding: 30px;
                    border: 1px solid #eee;
                    box-shadow: 0 0 10px rgba(0,0,0,.15);
                }}
                h1 {{
                    text-align: center;
                }}
                table {{
                    width: 100%;
                    border-collapse: collapse;
                    margin-top: 20px;
                }}
                th, td {{
                    padding: 10px;
                    border: 1px solid #ddd;
                    text-align: left;
                }}
                .totals td {{
                    font-weight: bold;
                }}
            </style>
        </head>
        <body>
            <div class=""invoice-box"">
                <h1>Invoice</h1>

                <p><strong>Invoice ID:</strong> {invoice.Id}</p>
                <p><strong>Ticket Serial:</strong> {invoice.TicketSerialNumber}</p>
                <p><strong>Ticket ID:</strong> {invoice.TicketId}</p>
                <p><strong>Date:</strong> {DateTime.UtcNow:yyyy-MM-dd}</p>

                <table>
                    <thead>
                        <tr>
                            <th>Description</th>
                            <th>Amount</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td>Total Before Tax</td>
                            <td>{invoice.TotalAmountBeforeTax:C}</td>
                        </tr>
                        <tr>
                            <td>Tax ({invoice.TaxAmount}%)</td>
                            <td>{(invoice.TotalAmountAfterTax - invoice.TotalAmountBeforeTax):C}</td>
                        </tr>
                        <tr class=""totals"">
                            <td>Total After Tax</td>
                            <td>{invoice.TotalAmountAfterTax:C}</td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </body>
        </html>";
    }
    // private async Task<string> SaveInvoiceHtmlDoc(string htmlDocument, string fileName)
    // {
    //     var uploader = new CloudinaryUploader();
    //     return await uploader.UploadHtmlAsync(htmlDocument, fileName);
    // }
}

