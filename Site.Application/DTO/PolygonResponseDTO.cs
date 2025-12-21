namespace Site.Application.DTO;

public class PolygonResponseDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public List<PolygonPointResponseDTO> PolygonPoints { get; set; } = new();
}
