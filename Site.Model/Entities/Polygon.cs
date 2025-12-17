namespace Site.Model.Entities;

public class Polygon
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    //relationData 
    public Guid SiteId { get; set; }
    public site Site { get; set; }

    public ICollection<PolygonPoint> PolygonPoints { get; set; } = new List<PolygonPoint>();
}