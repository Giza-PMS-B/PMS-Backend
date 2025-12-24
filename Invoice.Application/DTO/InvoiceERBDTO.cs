using System;

namespace Invoice.Application.DTO;

public class InvoiceERBDTO
{
    public Guid InvoiceId { get; set; }
    public Guid TicketId { get; set; }
    public string TicketSerial { get; set; }
    public DateTime BookingFrom { get; set; }
    public DateTime BookingTo { get; set; }
    public int NumOfHours { get; set; }
    public string PlateNumber { get; set; }
    public decimal TotalAmountBeforeTax { get; set; }
    public decimal TotalAmountAfterTax { get; set; }
    public string TicketSerialNumber { get; set; }
    public string InvoiceHTMLDoc { get; set; }

}
