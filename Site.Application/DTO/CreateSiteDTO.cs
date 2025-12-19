using System;

namespace Site.Application.DTO;

public class CreateSiteDTO
{
    public string Path { get; set; }
    public string NameEn { get; set; }
    public string NameAr { get; set; }
    public decimal PricePerHour { get; set; }
    public string IntegrationCode { get; set; }
    public int NumberOfSolts { get; set; }
    public bool IsLeaf { get; set; }
    public Guid? ParentId { get; set; }
    public List<CreatePolygonDTO> Polygons { get; set; } = [];
}
