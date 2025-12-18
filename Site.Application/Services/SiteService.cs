using System;
using SharedKernel.Infrastructure.Persistent.Abstraction;
using Site.Model.Entities;

namespace Site.Application.Services;

public class SiteService
{
    private readonly IRepo<Model.Entities.Site> _siteRepository;
    private readonly IUOW _uow;

    public SiteService(IRepo<Model.Entities.Site> siteRepository, IUOW uow)
    {
        _siteRepository = siteRepository;
        _uow = uow;
    }

    public async Task<Model.Entities.Site> CreateSiteAsync()
    {
        var site = new Model.Entities.Site
        {
            Id = Guid.NewGuid(),
            Path = "/location/to/site1",
            NameEn = "Sample Site 1",
            NameAr = "موقع تجريبمي",
            PricePerHour = 100,
            IntegrationCode = "INTG823",
            NumberOfSolts = 50,
            ParentId = null
        };

        for (int i = 0; i < 1; i++)
        {
            var polygon = new Polygon
            {
                Id = Guid.NewGuid(),
                Name = "PolygoncsA1",
                Site = site
            };

            for (int j = 0; j < 3; j++)
            {
                var pointDto = new
                {
                    Latitude = 11.0 + j,
                    Longitude = 21.0 + j
                };
                {
                    polygon.PolygonPoints.Add(new PolygonPoint
                    {
                        PolygonId = polygon.Id,
                        Latitude = (decimal)pointDto.Latitude,
                        Longitude = (decimal)pointDto.Longitude
                    });
                }

                site.Polygons.Add(polygon);
            }
        }
        await _siteRepository.AddAsync(site);
        await _uow.SaveChangesAsync();
        return site;
    }

}
