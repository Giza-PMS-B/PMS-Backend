using System;

namespace Invoice.Model.Entities;

public class Ticket
{
    public Guid Id { get; set; }
    public string SiteName { get; set; }
    public string PlateNumber { get; set; }
    public string PhoneNumber { get; set; }
    public DateTime BookingFrom { get; set; }
    public DateTime BookingTo { get; set; }
    public decimal TotalPrice { get; set; }

    // Navigation property
    public Invoice Invoice { get; set; }
}
