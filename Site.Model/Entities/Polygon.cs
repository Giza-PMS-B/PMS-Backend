using System.ComponentModel.DataAnnotations;

namespace Site.Model.Entities;

public class Polygon
{
    public Guid Id { get; set; }
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; }
    //relationData 
    public Guid SiteId { get; set; }
    public Site Site { get; set; }

    public ICollection<PolygonPoint> PolygonPoints { get; set; } = new List<PolygonPoint>();
}