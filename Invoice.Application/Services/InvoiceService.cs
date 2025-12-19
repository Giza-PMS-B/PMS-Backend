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
        throw new NotImplementedException();
    }
}
