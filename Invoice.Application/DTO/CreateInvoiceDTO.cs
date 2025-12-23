using System;

namespace Invoice.Application.DTO;

public class CreateInvoiceDTO
{
    public string HtmlDocumentPath { get; set; }
    public decimal TotalAmountAfterTax { get; set; }
    public string TicketSerialNumber { get; set; }

    public Guid TicketId { get; set; }
}
