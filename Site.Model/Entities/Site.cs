using System.ComponentModel.DataAnnotations;

namespace Site.Model.Entities;

public class site
{
    public Guid Id { get; set; }
    public string Path { get; set; }

    [StringLength(100, MinimumLength = 3)]
    public string NameEn { get; set; }
    [StringLength(100, MinimumLength = 3)]
    public string NameAr { get; set; }
    public decimal Price { get; set; }
    public string IntegrationCode { get; set; }
    public int NumberOfSolts { get; set; }
    private bool IsLeaf { get; set; }

    //relation attributes 
    public Guid? ParentId { get; set; }
    public site? Parent {get ;set;}
    public ICollection<site> Children { get; set; } = new List<site>();
    public ICollection<Polygon> Polygons { get; set; } = new List<Polygon>();
}