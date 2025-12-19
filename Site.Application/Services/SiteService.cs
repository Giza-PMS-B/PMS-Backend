using System;
using System.ComponentModel.DataAnnotations;
using SharedKernel.Infrastructure.Persistent.Abstraction;
using Site.Application.DTO;
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

    public async Task<Model.Entities.Site> CreateSiteAsync(CreateSiteDTO createSiteDTO)
    {
        if (createSiteDTO.Polygons == null || !createSiteDTO.Polygons.Any())
            throw new ValidationException("Site must have at least one polygon.");


        foreach (var polygon in createSiteDTO.Polygons)
        {
            if (polygon.Points == null || polygon.Points.Count < 3)
                throw new ValidationException("Polygon must have at least 3 points.");
        }

        var site = new Model.Entities.Site
        {
            Id = Guid.NewGuid(),
            Path = createSiteDTO.Path,
            NameEn = createSiteDTO.NameEn,
            NameAr = createSiteDTO.NameAr,
            PricePerHour = createSiteDTO.PricePerHour,
            IntegrationCode = createSiteDTO.IntegrationCode,
            NumberOfSolts = createSiteDTO.NumberOfSolts,
            IsLeaf = createSiteDTO.IsLeaf,
            ParentId = createSiteDTO.ParentId
        };

        foreach (var polygonDto in createSiteDTO.Polygons)
        {
            var polygon = new Polygon
            {
                Id = Guid.NewGuid(),
                Name = polygonDto.Name,
                Site = site
            };

            foreach (var pointDto in polygonDto.Points)
            {
                polygon.PolygonPoints.Add(new PolygonPoint
                {
                    PolygonId = polygon.Id,
                    Latitude = pointDto.Latitude,
                    Longitude = pointDto.Longitude
                });
            }

            site.Polygons.Add(polygon);
        }


        await _siteRepository.AddAsync(site);
        await _uow.SaveChangesAsync();
        return site;
    }

}
