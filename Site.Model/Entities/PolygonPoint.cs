namespace Site.Model.Entities;

public class PolygonPoint
{
    public decimal Longitude { get; set; }
    public decimal Latitude { get; set; }
    //realtion 
    public Guid PolygonId { get; set; }
    public Polygon Polygon { get; set; }
}