using System;

namespace Booking.Application.DTO;


public class SiteResponseDTO
{
    public Guid Id { get; set; }
    public string Path { get; set; }
    public string NameEn { get; set; }
    public string NameAr { get; set; }
    public decimal? PricePerHour { get; set; }
    public string? IntegrationCode { get; set; }
    public int? NumberOfSolts { get; set; }
    public bool IsLeaf { get; set; }
}




