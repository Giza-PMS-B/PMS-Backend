using System;

namespace Invoice.Application.DTO;

public class CreateInvoiceDTO
{
    public string HtmlDocument { get; set; }
    public decimal TotalAmountAfterTax { get; set; }
    public Guid TicketId { get; set; }
}
