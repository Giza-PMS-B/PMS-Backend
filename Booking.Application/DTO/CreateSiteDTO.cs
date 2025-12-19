using System;

namespace Booking.Application.DTO;

public class CreateSiteDTO
{
    public string Path { get; set; }
    public string NameEn { get; set; }
    public string NameAr { get; set; }
    public decimal PricePerHour { get; set; }
    public string IntegrationCode { get; set; }
    public int NumberOfSlots { get; set; }
    public bool IsLeaf { get; set; }
}
