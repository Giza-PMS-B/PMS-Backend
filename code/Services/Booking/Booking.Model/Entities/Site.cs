using System;
using System.ComponentModel.DataAnnotations;

namespace Booking.Model.Entities;

public class Site
{
    public Guid Id { get; set; }
    public string Path { get; set; }

    [StringLength(100, MinimumLength = 3)]
    public string NameEn { get; set; }

    [StringLength(100, MinimumLength = 3)]
    public string NameAr { get; set; }
    public decimal? PricePerHour { get; set; }
    public string? IntegrationCode { get; set; }
    public int? NumberOfSolts { get; set; }
    public bool IsLeaf { get; set; }

    //relation attributes
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
