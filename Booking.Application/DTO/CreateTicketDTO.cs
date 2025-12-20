using System;

namespace Booking.Application.DTO;

public class CreateTicketDTO
{
    public string SiteName { get; set; }      
    public string PlateNumber { get; set; }   
    public string PhoneNumber { get; set; }   
    public decimal TotalPrice { get; set; }
    public Guid SiteId { get; set; }
    public int NoOfHours { get; set; }
}
