using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Site.Application.DTO
{
    public class CreateLeafSiteDTO : CreateSiteDTO
    {
        public decimal PricePerHour { get; set; }
        public string IntegrationCode { get; set; }
        public int NumberOfSolts { get; set; }

        public List<CreatePolygonDTO> Polygons { get; set; } = [];
    }
}
