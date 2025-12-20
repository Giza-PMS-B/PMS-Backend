using System;
using Invoice.Application.DTO;
using SharedKernel.Infrastructure.Persistent.Abstraction;

namespace Invoice.Application.Services;

public class InvoiceService
{
    private readonly IRepo<Model.Entities.Invoice> _invoiceRepository;
    private readonly IUOW _uow;

    public InvoiceService(IRepo<Model.Entities.Invoice> invoiceRepository, IUOW uow)
    {
        _invoiceRepository = invoiceRepository;
        _uow = uow;
    }
    public async Task<Model.Entities.Invoice> CreateInvoiceAsync(CreateInvoiceDTO createInvoiceDTO)
    {
        var invoice = new Model.Entities.Invoice
        {
            Id = Guid.NewGuid(),
            HtmlDocumentPath = createInvoiceDTO.HtmlDocumentPath,
            TaxAmount = createInvoiceDTO.TaxAmount,
            TotalAmountBeforeTax = createInvoiceDTO.TotalAmountBeforeTax,
            TotalAmountAfterTax = createInvoiceDTO.TotalAmountAfterTax,
            TicketSerialNumber = createInvoiceDTO.TicketSerialNumber,
            TicketId = createInvoiceDTO.TicketId
        };

        await _invoiceRepository.AddAsync(invoice);
        await _uow.SaveChangesAsync();

        return invoice;
    }
}
