using System;

namespace Invoice.Model.Entities;

public class Invoice
{
    public Guid Id { get; set; }
    public string HtmlDocumentPath { get; set; }

    public decimal TaxAmount { get; set; }
    public decimal TotalAmountBeforeTax { get; set; }
    public decimal TotalAmountAfterTax { get; set; }

    public string TicketSerialNumber { get; set; }

    // Foreign key 
    public Guid TicketId { get; set; }
    // Navigation property
    public Ticket Ticket { get; set; }

}
