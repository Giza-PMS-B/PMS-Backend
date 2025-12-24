using System;

namespace Invoice.Application.DTO;

public class CreateInvoiceDTO
{
    public decimal TotalAmountAfterTax { get; set; }
    public Guid TicketId { get; set; }
}
