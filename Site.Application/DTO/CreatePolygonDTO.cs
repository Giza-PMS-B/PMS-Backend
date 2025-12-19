using System;

namespace Site.Application.DTO;

public class CreatePolygonDTO
{
    public string Name { get; set; }
    public List<CreatePolygonPointDTO> Points { get; set; } = [];
}
