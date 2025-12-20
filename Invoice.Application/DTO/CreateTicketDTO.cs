using System;

namespace Invoice.Application.DTO;

public class CreateTicketDTO
{
    public string SiteName { get; set; }
    public string PlateNumber { get; set; }
    public string PhoneNumber { get; set; }
    public DateTime BookingFrom { get; set; }
    public DateTime BookingTo { get; set; }
    public decimal TotalPrice { get; set; }
}
