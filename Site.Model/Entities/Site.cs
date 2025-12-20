using System.ComponentModel.DataAnnotations;

namespace Site.Model.Entities;

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
    public Guid? ParentId { get; set; }
    public Site? Parent { get; set; }
    public ICollection<Site> Children { get; set; } = new List<Site>();
    public ICollection<Polygon> Polygons { get; set; } = new List<Polygon>();
}